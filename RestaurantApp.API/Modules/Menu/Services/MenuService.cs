using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Menu.DTOs;
using RestaurantApp.API.Modules.Menu.Models;

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
            return await _context.MenuItems.Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId).OrderBy(i => i.SortOrder)
                .Select(i => ToItemDto(i)).ToListAsync();
        }

        public async Task<List<MenuItemDto>> GetAllItemsByRestaurantAsync(Guid restaurantId)
        {
            return await _context.MenuItems.Include(i => i.Category)
                .Where(i => i.Category!.RestaurantId == restaurantId)
                .OrderBy(i => i.Category!.SortOrder).ThenBy(i => i.SortOrder)
                .Select(i => ToItemDto(i)).ToListAsync();
        }

        public async Task<MenuItemDto?> GetItemByIdAsync(Guid id)
        {
            var i = await _context.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
            return i == null ? null : ToItemDto(i);
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
            };
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            return await GetItemByIdAsync(item.Id) ?? throw new Exception("Khong the tao mon");
        }

        public async Task<MenuItemDto?> UpdateItemAsync(Guid id, UpdateMenuItemDto dto)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return null;
            if (dto.Name != null) item.Name = dto.Name;
            if (dto.Description != null) item.Description = dto.Description;
            if (dto.Price.HasValue) item.BasePrice = dto.Price.Value;
            if (dto.ImageUrl != null) item.ImageUrl = dto.ImageUrl;
            if (dto.Unit != null) item.Unit = dto.Unit;
            if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;
            if (dto.SortOrder.HasValue) item.SortOrder = dto.SortOrder.Value;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetItemByIdAsync(id);
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return false;
            item.IsDeleted = true; item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(); return true;
        }

        private static MenuItemDto ToItemDto(MenuItem i) => new()
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
        };
    }
}
