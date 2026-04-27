using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Branch.Models;
using RestaurantApp.API.Modules.Menu.Models;
using RestaurantApp.API.Modules.Order.DTOs;
using RestaurantApp.API.Modules.Order.Models;
using RestaurantApp.API.Modules.Promotion.Services;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RestaurantApp.API.Hubs;
using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Order.Services
{
    public interface IOrderService
    {
        Task<List<OrderSummaryDto>> GetAllOrders(Guid branchId);
        Task<PagedResult<OrderSummaryDto>> GetAllOrdersPaged(Guid branchId, string? status, PaginationParams @params);
        Task<List<OrderSummaryDto>> GetOrdersByRestaurant(Guid restaurantId);
        Task<PagedResult<OrderSummaryDto>> GetOrdersByRestaurantPaged(Guid restaurantId, string? status, PaginationParams @params);
        Task<List<OrderSummaryDto>> GetActiveOrdersAsync(Guid branchId);
        Task<List<OrderSummaryDto>> GetActiveOrdersInRestaurantAsync(Guid restaurantId);
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
        Task<List<DishSaleDto>> GetDishSalesStatsAsync(Guid branchId, string filter);
        Task<List<DishSaleDto>> GetDishSalesStatsInRestaurantAsync(Guid restaurantId, string filter);
        Task<List<OrderDto>> GetPendingOrderInTableAsync(Guid tableId);
        Task<OrderDto?> AddItemsAsync(Guid orderId, List<AddOrderItemDto> items, Guid staffId);
        Task<OrderDto> SplitTableAsync(SplitTableDto dto, Guid staffId);
        Task<OrderDto> MergeTablesAsync(MergeTablesDto dto, Guid staffId);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IPromotionService _promotionService;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderService(AppDbContext context, IPromotionService promotionService, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _promotionService = promotionService;
            _hubContext = hubContext;
        }

        private static OrderDto MapToDto(Models.Order o) => new OrderDto
        {
            Id = o.Id,
            OrderCode = o.OrderCode,
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
            Items = o.OrderItems
                .Where(i => i.Status != OrderItemStatus.Cancelled)
                .Select(i => new OrderItemDto
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
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<PagedResult<OrderSummaryDto>> GetAllOrdersPaged(Guid branchId, string? status, PaginationParams @params)
        {
            var query = _context.Orders.Where(o => o.BranchId == branchId);
            
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(@params.Search))
            {
                // Simple search by table number or status
                query = query.Where(o => o.Table!.TableNumber.ToString().Contains(@params.Search) || o.Note.Contains(@params.Search));
            }

            return await query.OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    CreatedAt = o.CreatedAt
                }).ToPagedResultAsync(@params.PageIndex, @params.PageSize);
        }

        public async Task<List<OrderSummaryDto>> GetOrdersByRestaurant(Guid restaurantId)
        {
            return await _context.Orders.Where(o => o.RestaurantId == restaurantId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<PagedResult<OrderSummaryDto>> GetOrdersByRestaurantPaged(Guid restaurantId, string? status, PaginationParams @params)
        {
            var query = _context.Orders.Where(o => o.RestaurantId == restaurantId);
            
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(@params.Search))
            {
                query = query.Where(o => o.Table!.TableNumber.ToString().Contains(@params.Search) || o.Note.Contains(@params.Search));
            }

            return await query.OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    CreatedAt = o.CreatedAt
                }).ToPagedResultAsync(@params.PageIndex, @params.PageSize);
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
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    HasAwaitingItems = o.OrderItems.Any(i => i.Status == OrderItemStatus.AwaitingConfirmation),
                    CreatedAt = o.CreatedAt
                }).ToListAsync();
        }

        public async Task<List<OrderSummaryDto>> GetActiveOrdersInRestaurantAsync(Guid restaurantId)
        {
            return await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .Where(o => o.RestaurantId == restaurantId
                    && o.Status != OrderStatus.Paid
                    && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    TableNumber = o.Table!.TableNumber,
                    Status = o.Status,
                    Subtotal = o.Subtotal,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled),
                    HasAwaitingItems = o.OrderItems.Any(i => i.Status == OrderItemStatus.AwaitingConfirmation),
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

        private async Task RecalculateOrderTotalsAsync(Models.Order order)
        {
            // Only items that are NOT AwaitingConfirmation or Cancelled are counted towards the subtotal
            order.Subtotal = order.OrderItems
                .Where(i => i.Status != OrderItemStatus.AwaitingConfirmation && i.Status != OrderItemStatus.Cancelled)
                .Sum(i => i.TotalPrice);

            // 1. Check for Bill-level promotions (Manual Voucher always wins if present)
            if (order.VoucherId == null)
            {
                // Auto-applied bill promotions
                var autoBillPromos = await _promotionService.GetActiveBillPromotionsAsync((Guid)order.RestaurantId);
                decimal bestDiscount = 0;
                foreach (PromotionDto p in autoBillPromos)
                {
                    if (order.Subtotal < p.MinOrderAmount) continue;
                    decimal currentDiscount = 0;
                    if (p.Type == "percentage")
                    {
                        currentDiscount = order.Subtotal * (p.DiscountValue / 100);
                        if (p.MaxDiscount.HasValue && currentDiscount > p.MaxDiscount.Value) 
                            currentDiscount = p.MaxDiscount.Value;
                    }
                    else if (p.Type == "fixed_amount")
                    {
                        currentDiscount = p.DiscountValue;
                    }
                    if (currentDiscount > bestDiscount) bestDiscount = currentDiscount;
                }
                order.DiscountAmount = Math.Round(bestDiscount, 0);
            }

            // 2. Taxes (10%)
            order.TaxAmount = Math.Round((order.Subtotal - order.DiscountAmount) * 0.10m, 0);
            
            // 3. Total
            order.TotalAmount = Math.Max(0, order.Subtotal - order.DiscountAmount + order.TaxAmount);
            order.UpdatedAt = DateTime.UtcNow;
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
                OrderCode = $"ĐH-{DateTime.Now:yyyyMMddHHmmss}-{table.TableNumber}",
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

            await RecalculateOrderTotalsAsync(order);

            _context.Orders.Add(order);

            // Process Inventory Subtraction
            await SubtractInventoryAsync(table.BranchId, order.OrderItems.ToList());

            // Cập nhật status bàn
            table.Status = Table.Models.TableStatus.Occupied;
            table.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            
            // Realtime notification for QR orders
            if (staffId == Guid.Empty)
            {
                var posGroup = "pos_" + order.BranchId.ToString().ToLower();
                var restaurantGroup = "restaurant_" + order.RestaurantId.ToString().ToLower();
                
                Console.WriteLine($"[SignalR] Sending NewOrder to Groups: {posGroup}, {restaurantGroup} - Table: {table.TableNumber}");
                
                await _hubContext.Clients.Group(posGroup).SendAsync("NewOrder", new { TableNumber = table.TableNumber, OrderId = order.Id });
            }

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
                        await RecalculateOrderTotalsAsync(freshOrder);
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

            if (staffId == Guid.Empty)
            {
                var orderInTable = await _context.Orders.Include(o => o.Table).FirstOrDefaultAsync(o => o.Id == orderId);
                if (orderInTable != null)
                {
                    var posGroup = "pos_" + orderInTable.BranchId.ToString().ToLower();
                    var restaurantGroup = "restaurant_" + orderInTable.RestaurantId.ToString().ToLower();
                    
                    Console.WriteLine($"[SignalR] Sending NewItems to Groups: {posGroup}, {restaurantGroup} - Table: {orderInTable.Table?.TableNumber}");
                    
                    await _hubContext.Clients.Group(posGroup).SendAsync("NewItems", new { TableNumber = orderInTable.Table?.TableNumber ?? 0, OrderId = orderId });
                    await _hubContext.Clients.Group(restaurantGroup).SendAsync("NewItems", new { TableNumber = orderInTable.Table?.TableNumber ?? 0, OrderId = orderId });
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
            var item = await _context.OrderItems
                .Include(i => i.Order)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return false;
            item.Status = status;
            item.UpdatedAt = DateTime.UtcNow;

            if (item.Order != null)
            {
                await RecalculateOrderTotalsAsync(item.Order);
            }

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

            var existingOrderItemIdsInKitchen = await _context.KitchenOrders
                .Where(ko => itemsToSend.Select(i => i.Id).Contains(ko.OrderItemId))
                .Select(ko => ko.OrderItemId)
                .ToListAsync();

            foreach (var item in itemsToSend)
            {
                item.Status = OrderItemStatus.Cooking;
                item.SentToKitchenAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;

                if (!existingOrderItemIdsInKitchen.Contains(item.Id))
                {
                    _context.KitchenOrders.Add(new KitchenOrder
                    {
                        Id = Guid.NewGuid(),
                        BranchId = order.BranchId,
                        OrderItemId = item.Id,
                        TableNumber = order.Table?.TableNumber ?? 0,
                        ItemName = item.MenuItem?.Name ?? "N/A",
                        Quantity = item.Quantity,
                        Status = KitchenOrderStatus.Pending,
                        ReceivedAt = DateTime.UtcNow
                    });
                }
            }

            order.Status = OrderStatus.Preparing;
            order.UpdatedAt = DateTime.UtcNow;

            await RecalculateOrderTotalsAsync(order);

            await _context.SaveChangesAsync();

            // Realtime notification for Kitchen
            var kitchenGroup = $"kitchen_{order.BranchId.ToString().ToLower()}";
            await _hubContext.Clients.Group(kitchenGroup).SendAsync("NewKitchenOrder", new { TableNumber = order.Table?.TableNumber ?? 0, OrderId = orderId });

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
        public async Task<List<DishSaleDto>> GetDishSalesStatsAsync(Guid branchId, string filter)
        {
            var now = DateTime.UtcNow;
            var startDate = now.Date;

            if (filter == "week")
            {
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                startDate = now.AddDays(-1 * diff).Date;
            }
            else if (filter == "month")
            {
                startDate = new DateTime(now.Year, now.Month, 1);
            }

            return await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.MenuItem)
                .Where(i => i.Order!.BranchId == branchId 
                    && i.Order.Status == OrderStatus.Paid // Only count paid orders
                    && i.CreatedAt >= startDate)
                .GroupBy(i => i.MenuItem!.Name)
                .Select(g => new DishSaleDto
                {
                    MenuItemName = g.Key,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(s => s.TotalQuantity)
                .ToListAsync();
        }
        public async Task<List<DishSaleDto>> GetDishSalesStatsInRestaurantAsync(Guid restaurantId, string filter)
        {
            var now = DateTime.UtcNow;
            var startDate = now.Date;

            if (filter == "week")
            {
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                startDate = now.AddDays(-1 * diff).Date;
            }
            else if (filter == "month")
            {
                startDate = new DateTime(now.Year, now.Month, 1);
            }

            return await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.MenuItem)
                .Where(i => i.Order!.RestaurantId == restaurantId 
                    && i.Order.Status == OrderStatus.Paid 
                    && i.CreatedAt >= startDate)
                .GroupBy(i => i.MenuItem!.Name)
                .Select(g => new DishSaleDto
                {
                    MenuItemName = g.Key,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(s => s.TotalQuantity)
                .ToListAsync();
        }

        public async Task<List<OrderDto>> GetPendingOrderInTableAsync(Guid tableId)
        {
            var orders = await OrdersWithIncludes.Where(o => o.TableId == tableId && o.Status == OrderStatus.Pending).ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> SplitTableAsync(SplitTableDto dto, Guid staffId)
        {
            // 1. Get Source Order
            var sourceOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.TableId == dto.SourceTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

            if (sourceOrder == null) throw new Exception("Không tìm thấy đơn hàng tại bàn gốc hoặc đơn hàng đã thanh toán/hủy");

            // 2. Validate conditions
            if (sourceOrder.Status == OrderStatus.Paid) throw new Exception("Bàn đã in hóa đơn hoặc đã thanh toán, không thể tách");
            
            // 3. Get Target Table
            var targetTable = await _context.Tables.FindAsync(dto.TargetTableId);
            if (targetTable == null) throw new Exception("Không tìm thấy bàn đích");
            
            var existingTargetOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.TableId == dto.TargetTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

            Models.Order targetOrder;
            if (existingTargetOrder == null)
            {
                // Create new order for target table
                targetOrder = new Models.Order
                {
                    Id = Guid.NewGuid(),
                    BranchId = sourceOrder.BranchId,
                    TableId = dto.TargetTableId,
                    OrderCode = $"ĐH-{DateTime.Now:yyyyMMddHHmmss}-{targetTable.TableNumber}",
                    EmployeeId = staffId,
                    RestaurantId = sourceOrder.RestaurantId,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    GuestCount = 1
                };
                _context.Orders.Add(targetOrder);
                targetTable.Status = Table.Models.TableStatus.Occupied;
            }
            else
            {
                targetOrder = existingTargetOrder;
            }

            // 4. Move items
            foreach (var splitItem in dto.Items)
            {
                var sourceItem = sourceOrder.OrderItems.FirstOrDefault(i => i.Id == splitItem.OrderItemId);
                if (sourceItem == null) continue;

                if (splitItem.Quantity >= sourceItem.Quantity)
                {
                    // Move entire record
                    sourceItem.OrderId = targetOrder.Id;
                    sourceItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Split quantity
                    var newItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = targetOrder.Id,
                        MenuItemId = sourceItem.MenuItemId,
                        Quantity = splitItem.Quantity,
                        UnitPrice = sourceItem.UnitPrice,
                        TotalPrice = sourceItem.UnitPrice * splitItem.Quantity,
                        OriginalPrice = sourceItem.OriginalPrice,
                        Status = sourceItem.Status,
                        Note = sourceItem.Note,
                        CreatedAt = sourceItem.CreatedAt,
                        UpdatedAt = DateTime.UtcNow,
                        SentToKitchenAt = sourceItem.SentToKitchenAt
                    };
                    _context.OrderItems.Add(newItem);

                    sourceItem.Quantity -= splitItem.Quantity;
                    sourceItem.TotalPrice = sourceItem.UnitPrice * sourceItem.Quantity;
                    sourceItem.UpdatedAt = DateTime.UtcNow;
                }
            }

            await RecalculateOrderTotalsAsync(sourceOrder);
            await RecalculateOrderTotalsAsync(targetOrder);

            await _context.SaveChangesAsync();

            // Realtime Update via Hub
            var branchIdStr = sourceOrder.BranchId.ToString().ToLower();
            await _hubContext.Clients.Group("pos_" + branchIdStr).SendAsync("NewOrder", new { TableNumber = targetTable.TableNumber, OrderId = targetOrder.Id });

            return MapToDto(targetOrder);
        }

        public async Task<OrderDto> MergeTablesAsync(MergeTablesDto dto, Guid staffId)
        {
            // 1. Get Master Table and Order
            var masterTable = await _context.Tables.FindAsync(dto.MasterTableId);
            if (masterTable == null) throw new Exception("Không tìm thấy bàn chính");

            var masterOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.TableId == dto.MasterTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled);

            if (masterOrder == null)
            {
                // This shouldn't happen if we strictly follow "Choose a table with order as Master"
                // but let's handle case where we merge into an empty table (though DTO usually implies existing orders)
                throw new Exception("Bàn chính phải là bàn đang có đơn hàng");
            }

            // 2. Get Source Orders
            var sourceOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .Where(o => dto.SourceTableIds.Contains(o.TableId) && o.TableId != dto.MasterTableId && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            foreach (var sOrder in sourceOrders)
            {
                // Move all items
                foreach (var item in sOrder.OrderItems)
                {
                    item.OrderId = masterOrder.Id;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                sOrder.Status = OrderStatus.Cancelled; // Or a new status like 'Merged'
                sOrder.Note = (sOrder.Note ?? "") + $" [Đã gộp vào bàn {masterTable.TableNumber}]";
                
                var sTable = await _context.Tables.FindAsync(sOrder.TableId);
                if (sTable != null)
                {
                    sTable.Status = Table.Models.TableStatus.Available;
                }
            }

            await RecalculateOrderTotalsAsync(masterOrder);
            await _context.SaveChangesAsync();

            // Realtime update
            var branchIdStr = masterOrder.BranchId.ToString().ToLower();
            await _hubContext.Clients.Group("pos_" + branchIdStr).SendAsync("NewOrder", new { TableNumber = masterTable.TableNumber, OrderId = masterOrder.Id });

            return MapToDto(masterOrder);
        }

    }
}
            
           
       