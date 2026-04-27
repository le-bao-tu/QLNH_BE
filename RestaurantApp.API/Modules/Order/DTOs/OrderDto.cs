namespace RestaurantApp.API.Modules.Order.DTOs
{
    public class CreateOrderDto
    {
        public Guid TableId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid RestaurantId { get; set; }
        public int GuestCount { get; set; } = 1;
        public string? Note { get; set; }
        public string? VoucherCode { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class AddOrderItemDto
    {
        public Guid MenuItemId { get; set; }
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateOrderItemStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public Guid Id { get; set; }
        public Guid MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public string? MenuItemImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal OriginalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid TableId { get; set; }
        public int TableNumber { get; set; }
        public Guid? StaffId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int GuestCount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public int TableNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public bool HasAwaitingItems { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DishSaleDto
    {
        public string MenuItemName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SplitTableDto
    {
        public Guid SourceTableId { get; set; }
        public Guid TargetTableId { get; set; }
        public List<SplitItemDto> Items { get; set; } = new();
    }

    public class SplitItemDto
    {
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class MergeTablesDto
    {
        public List<Guid> SourceTableIds { get; set; } = new();
        public Guid MasterTableId { get; set; }
    }
}
