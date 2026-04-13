using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Branch.Models;
using RestaurantApp.API.Modules.Menu.Models;
using RestaurantApp.API.Modules.Order.DTOs;
using RestaurantApp.API.Modules.Order.Models;
using RestaurantApp.API.Modules.Promotion.Services;
using System.Text.Json;

namespace RestaurantApp.API.Modules.Order.Services
{
    public interface IOrderService
    {
        Task<List<OrderSummaryDto>> GetAllOrders(Guid branchId);
        Task<List<OrderSummaryDto>> GetOrdersByRestaurant(Guid branchId);
        Task<List<OrderSummaryDto>> GetActiveOrdersAsync(Guid branchId);
        Task<List<OrderDto>> GetByTableAsync(Guid tableId);
        Task<OrderDto?> GetByIdAsync(Guid id);
        Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid staffId);
        Task<OrderDto?> AddItemAsync(Guid orderId, AddOrderItemDto dto, Guid staffId);
        Task<OrderDto?> UpdateStatusAsync(Guid orderId, string status);
        Task<bool> UpdateItemStatusAsync(Guid itemId, string status);
        Task<bool> CancelOrderAsync(Guid orderId);
        Task<OrderDto?> ConfirmOrderAsync(Guid orderId);
        Task<bool> ConfirmItemAsync(Guid itemId);
        Task<bool> SendToKitchenAsync(Guid orderId);
        Task<List<OrderDto>> GetKitchenOrdersAsync(Guid branchId);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IPromotionService _promotionService;

        public OrderService(AppDbContext context, IPromotionService promotionService)
        {
            _context = context;
            _promotionService = promotionService;
        }

        private static OrderDto MapToDto(Models.Order o) => new OrderDto
        {
            Id = o.Id,
            TableId = o.TableId,
            TableNumber = o.Table?.TableNumber ?? 0,
            StaffId = o.EmployeeId ?? Guid.Empty,
            Status = o.Status,
            Note = o.Note,
            TotalAmount = o.TotalAmount,
            Subtotal = o.Subtotal,
            DiscountAmount = o.DiscountAmount,
            TaxAmount = o.TaxAmount,
            GuestCount = o.GuestCount,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = o.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                MenuItemId = i.MenuItemId,
                MenuItemName = i.MenuItem?.Name ?? "",
                MenuItemImageUrl = i.MenuItem?.ImageUrl,
                Quantity = i.Quantity,
                OriginalPrice = i.OriginalPrice,
                UnitPrice = i.UnitPrice,
                SubTotal = i.TotalPrice,
                Status = i.Status,
                Note = i.Note,
                CreatedAt = i.CreatedAt
            }).ToList()
        };

        private IQueryable<Models.Order> OrdersWithIncludes => _context.Orders
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem);

        public async Task<List<OrderSummaryDto>> GetAllOrders(Guid branchId)
        {
            return await _context.Orders.Where(o => o.BranchId == branchId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<List<OrderSummaryDto>> GetOrdersByRestaurant(Guid restaurantId)
        {
            return await _context.Orders.Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<List<OrderSummaryDto>> GetActiveOrdersAsync(Guid branchId)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .Where(o => o.BranchId == branchId
                    && o.Status != OrderStatus.Paid
                    && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<List<OrderDto>> GetByTableAsync(Guid tableId)
        {
            var orders = await OrdersWithIncludes
                .Where(o => o.TableId == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetByIdAsync(Guid id)
        {
            var order = await OrdersWithIncludes.FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? null : MapToDto(order);
        }

        private async Task SubtractInventoryAsync(Guid branchId, List<OrderItem> items)
        {
            var menuItemIds = items.Select(i => i.MenuItemId).ToList();
            var recipes = await _context.Set<RestaurantApp.API.Modules.Inventory.Models.MenuItemRecipe>()
                .Include(r => r.InventoryItem)
                .Where(r => menuItemIds.Contains(r.MenuItemId))
                .ToListAsync();

            if (!recipes.Any()) return;

            foreach (var item in items)
            {
                var itemRecipes = recipes.Where(r => r.MenuItemId == item.MenuItemId);
                foreach (var recipe in itemRecipes)
                {
                    if (recipe.InventoryItem == null) continue;

                    var amountToSubtract = recipe.QuantityUsed * item.Quantity;
                    recipe.InventoryItem.CurrentQuantity -= amountToSubtract;

                    // Add Inventory Transaction
                    _context.Set<RestaurantApp.API.Modules.Inventory.Models.InventoryTransaction>().Add(new RestaurantApp.API.Modules.Inventory.Models.InventoryTransaction
                    {
                        InventoryItemId = recipe.InventoryItemId,
                        Type = "export",
                        QuantityChange = -amountToSubtract,
                        QuantityAfter = recipe.InventoryItem.CurrentQuantity,
                        ReferenceId = item.OrderId,
                        Note = $"Xuất kho cho món: {item.MenuItem?.Name} (Số lượng order: {item.Quantity})",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto dto, Guid staffId)
        {
            var menuItemIds = dto.Items.Select(i => i.MenuItemId).ToList();
            var menuItems = await _context.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id);

            // Get branch from table
            var table = await _context.Tables.Include(t => t.Branch).FirstOrDefaultAsync(t => t.Id == dto.TableId);
            if (table == null) throw new Exception("Không tìm thấy bàn");

            var restaurantId = table.Branch?.RestaurantId ?? Guid.Empty;
            var activePromotions = await _promotionService.GetActiveItemPromotionsAsync(restaurantId);

            var order = new Models.Order
            {
                Id = Guid.NewGuid(),
                BranchId = table.BranchId,
                TableId = dto.TableId,
                EmployeeId = staffId == Guid.Empty ? null : staffId,
                CustomerId = dto.CustomerId,
                GuestCount = dto.GuestCount,
                Note = dto.Note,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                RestaurantId = dto.RestaurantId
            };

            foreach (var item in dto.Items)
            {
                if (!menuItems.TryGetValue(item.MenuItemId, out var menuItem)) continue;
                var unitPrice = CalculateUnitPrice(menuItem.Id, menuItem.BasePrice, activePromotions);
                var totalPrice = unitPrice * item.Quantity;
                var originalPrice = menuItem.BasePrice;

                order.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    MenuItemId = item.MenuItemId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    OriginalPrice = originalPrice,
                    Status = OrderItemStatus.Pending,
                    Note = item.Note,
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.Subtotal = order.OrderItems.Sum(i => i.TotalPrice);
            // Default 10% VAT (Step 6.2)
            order.TaxAmount = Math.Round(order.Subtotal * 0.10m, 0); 
            order.TotalAmount = order.Subtotal - order.DiscountAmount + order.TaxAmount;

            _context.Orders.Add(order);

            // Process Inventory Subtraction
            await SubtractInventoryAsync(table.BranchId, order.OrderItems.ToList());

            // Cập nhật status bàn
            table.Status = Table.Models.TableStatus.Occupied;
            table.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(order.Id) ?? throw new Exception("Khong the tao don hang");
        }

        private decimal CalculateUnitPrice(Guid menuItemId, decimal basePrice, List<PromotionDto> activePromotions)
        {
            var applicablePromotions = activePromotions.Where(p => 
            {
                if (string.IsNullOrEmpty(p.MenuItemIds)) return false;
                try 
                {
                    var ids = JsonSerializer.Deserialize<List<string>>(p.MenuItemIds);
                    return ids != null && ids.Contains(menuItemId.ToString());
                }
                catch { return p.MenuItemIds.Contains(menuItemId.ToString()); }
            }).ToList();

            if (!applicablePromotions.Any()) return basePrice;

            // Apply the best promotion (lowest price)
            decimal minPrice = basePrice;
            foreach (var p in applicablePromotions)
            {
                decimal discountedPrice = basePrice;
                if (p.Type == "percentage")
                {
                    discountedPrice = basePrice * (1 - p.DiscountValue / 100);
                }
                else if (p.Type == "fixed_amount")
                {
                    discountedPrice = Math.Max(0, basePrice - p.DiscountValue);
                }
                
                if (discountedPrice < minPrice) minPrice = discountedPrice;
            }

            return minPrice;
        }

        public async Task<OrderDto?> AddItemsAsync(Guid orderId, List<AddOrderItemDto> items, Guid staffId)
        {
            // Tải Order và các ID món ăn liên quan
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return null;

            var branch = await _context.Branches.FindAsync(order.BranchId);
            var restaurantId = branch?.RestaurantId ?? Guid.Empty;
            var activePromotions = await _promotionService.GetActiveItemPromotionsAsync(restaurantId);

            var menuItemIds = items.Select(i => i.MenuItemId).ToList();
            var menuItems = await _context.MenuItems.Where(m => menuItemIds.Contains(m.Id)).ToDictionaryAsync(m => m.Id);

            var newItems = new List<OrderItem>();
            foreach (var dto in items)
            {
                if (!menuItems.TryGetValue(dto.MenuItemId, out var menuItem)) continue;

                var unitPrice = CalculateUnitPrice(menuItem.Id, menuItem.BasePrice, activePromotions);
                var totalPrice = unitPrice * dto.Quantity;

                var newItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    MenuItemId = dto.MenuItemId,
                    Quantity = dto.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    OriginalPrice = menuItem.BasePrice,
                    Status = staffId == Guid.Empty ? OrderItemStatus.AwaitingConfirmation : OrderItemStatus.Pending,
                    Note = dto.Note,
                    CreatedAt = DateTime.UtcNow
                };
                newItems.Add(newItem);
                _context.OrderItems.Add(newItem); // Thêm trực tiếp vào context
            }

            // Xử lý kho (nếu có công thức)
            await SubtractInventoryAsync(order.BranchId, newItems);

            // Lưu món ăn trước
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear(); // Dọn dẹp context

            // Bây giờ mới cập nhật tổng tiền cho Order (với logic thử lại nếu lỗi)
            const int maxRetries = 5;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    var freshOrder = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
                    if (freshOrder != null)
                    {
                        freshOrder.Subtotal = freshOrder.OrderItems.Sum(oi => oi.TotalPrice);
                        freshOrder.TaxAmount = Math.Round(freshOrder.Subtotal * 0.10m, 0);
                        freshOrder.TotalAmount = freshOrder.Subtotal - freshOrder.DiscountAmount + freshOrder.TaxAmount;
                        freshOrder.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        break;
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(100 * (i + 1));
                    _context.ChangeTracker.Clear();
                }
            }

            return await GetByIdAsync(orderId);
        }

        public async Task<OrderDto?> AddItemAsync(Guid orderId, AddOrderItemDto dto, Guid staffId)
        {
            return await AddItemsAsync(orderId, new List<AddOrderItemDto> { dto }, staffId);
        }

        public async Task<OrderDto?> UpdateStatusAsync(Guid orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return null;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            if (status == OrderStatus.Paid || status == OrderStatus.Cancelled)
            {
                var table = await _context.Tables.FindAsync(order.TableId);
                if (table != null) { table.Status = Table.Models.TableStatus.Available; table.UpdatedAt = DateTime.UtcNow; }
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(orderId);
        }

        public async Task<bool> UpdateItemStatusAsync(Guid itemId, string status)
        {
            var item = await _context.OrderItems.FindAsync(itemId);
            if (item == null) return false;
            item.Status = status;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            foreach (var item in order.OrderItems) item.Status = OrderItemStatus.Cancelled;

            var table = await _context.Tables.FindAsync(order.TableId);
            if (table != null) { table.Status = Table.Models.TableStatus.Available; table.UpdatedAt = DateTime.UtcNow; }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrderDto?> ConfirmOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return null;

            order.Status = OrderStatus.Pending;
            order.UpdatedAt = DateTime.UtcNow;

            foreach (var item in order.OrderItems.Where(i => i.Status == OrderItemStatus.AwaitingConfirmation))
            {
                item.Status = OrderItemStatus.Pending;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(orderId);
        }

        public async Task<bool> ConfirmItemAsync(Guid itemId)
        {
            var item = await _context.OrderItems
                .Include(i => i.Order)
                    .ThenInclude(o => o.Table)
                .Include(i => i.MenuItem)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null || item.Status != OrderItemStatus.AwaitingConfirmation) return false;

            item.Status = OrderItemStatus.Pending;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SendToKitchenAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            var itemsToSend = order.OrderItems.Where(i => i.Status == OrderItemStatus.Pending).ToList();
            if (!itemsToSend.Any()) return true;

            foreach (var item in itemsToSend)
            {
                item.Status = OrderItemStatus.Cooking;
                item.SentToKitchenAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                _context.KitchenOrders.Add(new KitchenOrder
                {
                    Id = Guid.NewGuid(),
                    BranchId = order.BranchId,
                    OrderItem = item,
                    TableNumber = order.Table?.TableNumber ?? 0,
                    ItemName = item.MenuItem?.Name ?? "N/A",
                    Quantity = item.Quantity,
                    Status = KitchenOrderStatus.Pending, // Kitchen ticket starts as pending
                    ReceivedAt = DateTime.UtcNow
                });
            }

            order.Status = OrderStatus.Preparing;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<OrderDto>> GetKitchenOrdersAsync(Guid branchId)
        {
            var orders = await OrdersWithIncludes
                .Where(o => o.BranchId == branchId
                    && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Preparing))
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            var result = orders.Select(MapToDto).ToList();
            // Filter each order's items to only show Cooking ones for the kitchen view
            foreach (var r in result)
            {
                r.Items = r.Items.Where(i => i.Status == OrderItemStatus.Cooking).ToList();
            }
            
            return result.Where(r => r.Items.Any()).ToList();
        }
    }
}
            
           
       