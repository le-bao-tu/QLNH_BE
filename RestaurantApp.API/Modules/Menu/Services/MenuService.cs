using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Menu.DTOs;
using RestaurantApp.API.Modules.Menu.Models;
using RestaurantApp.API.Modules.Promotion.Models;
using RestaurantApp.API.Modules.Restaurant.Models;
using System.Text.Json;

namespace RestaurantApp.API.Modules.Menu.Services
{
    public interface IMenuService
    {
        Task<List<MenuCategoryDto>> GetCategoriesByRestaurantAsync(Guid restaurantId);
        Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto dto);
        Task<MenuCategoryDto?> UpdateCategoryAsync(Guid id, UpdateMenuCategoryDto dto);
        Task<bool> DeleteCategoryAsync(Guid id);

        Task<List<MenuItemDto>> GetItemsByCategoryAsync(Guid categoryId);
        Task<List<MenuItemDto>> GetAllItemsByRestaurantAsync(Guid restaurantId);
        Task<List<MenuItemDto>> GetAllItemsByBranchAsync(Guid branchId);
        Task<MenuItemDto?> GetItemByIdAsync(Guid id);
        Task<MenuItemDto> CreateItemAsync(CreateMenuItemDto dto);
        Task<MenuItemDto?> UpdateItemAsync(Guid id, UpdateMenuItemDto dto);
        Task<bool> DeleteItemAsync(Guid id);
    }

    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;

        public MenuService(AppDbContext context) => _context = context;

        public async Task<List<MenuCategoryDto>> GetCategoriesByRestaurantAsync(Guid restaurantId)
        {
            return await _context.MenuCategories
                .Where(c => c.RestaurantId == restaurantId)
                .OrderBy(c => c.SortOrder)
                .Select(c => new MenuCategoryDto
                {
                    Id = c.Id,
                    RestaurantId = c.RestaurantId,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    Items = c.MenuItems
                        .Where(i => !i.IsDeleted)
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new MenuItemDto
                        {
                            Id = i.Id,
                            CategoryId = i.CategoryId,
                            CategoryName = c.Name,
                            Name = i.Name,
                            Description = i.Description,
                            Price = i.BasePrice,
                            ImageUrl = i.ImageUrl,
                            Unit = i.Unit,
                            IsAvailable = i.IsAvailable,
                            SortOrder = i.SortOrder
                        }).ToList()
                }).ToListAsync();
        }

        public async Task<MenuCategoryDto> CreateCategoryAsync(CreateMenuCategoryDto dto)
        {
            var cat = new MenuCategory
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                SortOrder = dto.SortOrder,
                IsActive = true,
            };
            _context.MenuCategories.Add(cat);
            await _context.SaveChangesAsync();
            return new MenuCategoryDto { Id = cat.Id, RestaurantId = cat.RestaurantId, Name = cat.Name, ImageUrl = cat.ImageUrl, SortOrder = cat.SortOrder, IsActive = cat.IsActive };
        }

        public async Task<MenuCategoryDto?> UpdateCategoryAsync(Guid id, UpdateMenuCategoryDto dto)
        {
            var cat = await _context.MenuCategories.FindAsync(id);
            if (cat == null) return null;
            if (dto.Name != null) cat.Name = dto.Name;
            if (dto.ImageUrl != null) cat.ImageUrl = dto.ImageUrl;
            if (dto.SortOrder.HasValue) cat.SortOrder = dto.SortOrder.Value;
            if (dto.IsActive.HasValue) cat.IsActive = dto.IsActive.Value;
            cat.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new MenuCategoryDto { Id = cat.Id, RestaurantId = cat.RestaurantId, Name = cat.Name, ImageUrl = cat.ImageUrl, SortOrder = cat.SortOrder, IsActive = cat.IsActive };
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var cat = await _context.MenuCategories.FindAsync(id);
            if (cat == null) return false;
            cat.IsDeleted = true; cat.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<List<MenuItemDto>> GetItemsByCategoryAsync(Guid categoryId)
        {
            var category = await _context.MenuCategories.FindAsync(categoryId);
            var restaurantId = category?.RestaurantId ?? Guid.Empty;
            var activePromotions = await GetActiveItemPromotionsAsync(restaurantId, Guid.Empty);

            return await _context.MenuItems
                .Include(i => i.Category)
                .Include(i => i.ComboItems).ThenInclude(c => c.SingleItem)
                .Where(i => i.CategoryId == categoryId).OrderBy(i => i.SortOrder)
                .Select(i => ToItemDto(i, activePromotions)).ToListAsync();
        }

        public async Task<List<MenuItemDto>> GetAllItemsByRestaurantAsync(Guid restaurantId)
        {
            var activePromotions = await GetActiveItemPromotionsAsync(restaurantId, Guid.Empty);

            var menuItems = await _context.MenuItems
                .Include(i => i.Category)
                .Include(i => i.ComboItems).ThenInclude(c => c.SingleItem)
                .Where(i => i.Category!.RestaurantId == restaurantId && !i.IsDeleted)
                .OrderBy(i => i.Category!.SortOrder).ThenBy(i => i.SortOrder)
                .ToListAsync();

            return menuItems.Select(i => ToItemDto(i, activePromotions)).ToList();
        }

        public async Task<List<MenuItemDto>> GetAllItemsByBranchAsync(Guid branchId)
        {
            var branch = _context.Branches.Find(branchId);

            if (branch == null) throw new Exception("Không tìm thấy chi nhánh với id: " + branchId);

            var activePromotions = await GetActiveItemPromotionsAsync(branch.RestaurantId, branchId);

            var menuItems = await _context.MenuItems
                .Include(i => i.Category)
                .Include(i => i.ComboItems).ThenInclude(c => c.SingleItem)
                .Where(i => i.BranchIds!.Contains(branchId.ToString()) && !i.IsDeleted)
                .OrderBy(i => i.Category!.SortOrder).ThenBy(i => i.SortOrder)
                .ToListAsync();

            return menuItems.Select(i => ToItemDto(i, activePromotions)).ToList();
        }


        private async Task<List<Modules.Promotion.Models.Promotion>> GetActiveItemPromotionsAsync(Guid restaurantId, Guid branchId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _context.Promotions
                .Where(p => (p.RestaurantId == restaurantId || restaurantId == Guid.Empty) 
                    && (p.BranchIds!.Contains(branchId.ToString()) || branchId == Guid.Empty)
                    && p.IsActive
                    && !p.IsDeleted
                    && p.ApplyTo == "item"
                    && p.StartDate <= today
                    && p.EndDate >= today)
                .ToListAsync();
        }

        public async Task<MenuItemDto?> GetItemByIdAsync(Guid id)
        {
            var i = await _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.ComboItems).ThenInclude(c => c.SingleItem)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (i == null) return null;
            
            var restaurantId = i.Category?.RestaurantId ?? Guid.Empty;
            var activePromotions = await GetActiveItemPromotionsAsync(restaurantId, Guid.Empty);
            
            return ToItemDto(i, activePromotions);
        }

        public async Task<MenuItemDto> CreateItemAsync(CreateMenuItemDto dto)
        {
            var item = new MenuItem
            {
                CategoryId = dto.CategoryId,
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.Price,
                ImageUrl = dto.ImageUrl,
                Unit = dto.Unit ?? "phan",
                SortOrder = dto.SortOrder,
                IsAvailable = true,
                ItemType = dto.ItemType ?? "single",
                BranchIds = dto.BranchIds
            };

            // Save item first to get the generated ID before adding combo children
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            if (item.ItemType == "combo" && dto.ComboItemIds != null && dto.ComboItemIds.Count > 0)
            {
                foreach (var sId in dto.ComboItemIds)
                {
                    _context.MenuItemCombos.Add(new MenuItemCombo
                    {
                        ComboItemId = item.Id,   // now item.Id is a real GUID
                        SingleItemId = sId,
                        Quantity = 1
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetItemByIdAsync(item.Id) ?? throw new Exception("Khong the tao mon");
        }

        public async Task<MenuItemDto?> UpdateItemAsync(Guid id, UpdateMenuItemDto dto)
        {
            var item = await _context.MenuItems
                .Include(m => m.ComboItems)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return null;

            if (dto.Name != null) item.Name = dto.Name;
            if (dto.Description != null) item.Description = dto.Description;
            if (dto.Price.HasValue) item.BasePrice = dto.Price.Value;
            if (dto.ImageUrl != null) item.ImageUrl = dto.ImageUrl;
            if (dto.Unit != null) item.Unit = dto.Unit;
            if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;
            if (dto.SortOrder.HasValue) item.SortOrder = dto.SortOrder.Value;
            if (dto.ItemType != null) item.ItemType = dto.ItemType;
            if (dto.CategoryId != null) item.CategoryId = dto.CategoryId.Value;
            if (!string.IsNullOrEmpty(dto.BranchIds)) item.BranchIds = dto.BranchIds;

            item.UpdatedAt = DateTime.UtcNow;

            if (dto.ComboItemIds != null && item.ItemType == "combo")
            {
                // Bước 1: Xóa hết combo cũ trước
                _context.Set<MenuItemCombo>().RemoveRange(item.ComboItems);
                await _context.SaveChangesAsync(); // ← commit delete trước

                // Bước 2: Thêm mới
                var newCombos = dto.ComboItemIds.Select(sId => new MenuItemCombo
                {
                    ComboItemId = item.Id,
                    SingleItemId = sId,
                    Quantity = 1
                }).ToList();

                await _context.Set<MenuItemCombo>().AddRangeAsync(newCombos);
            }

            await _context.SaveChangesAsync(); // ← commit update + insert mới
            return await GetItemByIdAsync(id);
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true; item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(); return true;
        }

        private MenuItemDto ToItemDto(MenuItem i, List<Modules.Promotion.Models.Promotion> activePromotions)
        {
            var dto = new MenuItemDto
            {
                Id = i.Id,
                CategoryId = i.CategoryId,
                CategoryName = i.Category?.Name ?? "",
                Name = i.Name,
                Description = i.Description ?? "",
                Price = i.BasePrice,
                ImageUrl = i.ImageUrl,
                Unit = i.Unit,
                IsAvailable = i.IsAvailable,
                SortOrder = i.SortOrder,
                ItemType = i.ItemType,
                BranchIds = i.BranchIds,
                ComboItems = i.ComboItems?.Where(c => c.SingleItem != null).Select(c => new MenuComboItemDto
                {
                    Id = c.SingleItem!.Id,
                    Name = c.SingleItem.Name,
                    Price = c.SingleItem.BasePrice,
                    ImageUrl = c.SingleItem.ImageUrl,
                    Quantity = c.Quantity
                }).ToList()
            };

            // Calculate discounts
            var applicablePromotions = activePromotions.Where(p =>
            {
                if (string.IsNullOrEmpty(p.MenuItemIds)) return false;
                try
                {
                    var ids = JsonSerializer.Deserialize<List<string>>(p.MenuItemIds);
                    return ids != null && ids.Contains(i.Id.ToString());
                }
                catch { return p.MenuItemIds.Contains(i.Id.ToString()); }
            }).ToList();

            if (applicablePromotions.Any())
            {
                // Find the best promotion (lowest price)
                var bestPromo = applicablePromotions.OrderBy(p => 
                {
                    if (p.Type == PromotionType.PercentDiscount) return i.BasePrice * (1 - p.DiscountValue / 100);
                    if (p.Type == PromotionType.FixedDiscount) return Math.Max(0, i.BasePrice - p.DiscountValue);
                    return i.BasePrice;
                }).First();

                dto.DiscountType = bestPromo.Type;
                dto.DiscountValue = bestPromo.DiscountValue;
                
                if (bestPromo.Type == PromotionType.PercentDiscount)
                    dto.DiscountPrice = i.BasePrice * (1 - bestPromo.DiscountValue / 100);
                else
                    dto.DiscountPrice = Math.Max(0, i.BasePrice - bestPromo.DiscountValue);
            }
            else
            {
                dto.DiscountPrice = i.BasePrice;
            }

            return dto;
        }
    }
}
