using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Order.Models
{
    /// <summary>Đơn hàng tại bàn</summary>
    public class Order : BaseEntity
    {
        /// <summary>Chi nhánh phục vụ</summary>
        public Guid BranchId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Bàn phục vụ</summary>
        public Guid TableId { get; set; }
        public Table.Models.Table? Table { get; set; }

        /// <summary>Khách hàng (nếu có)</summary>
        public Guid? CustomerId { get; set; }
        public Customer.Models.Customer? Customer { get; set; }

        /// <summary>Nhân viên tạo đơn</summary>
        public Guid? EmployeeId { get; set; }

        /// <summary>Trạng thái đơn: pending/preparing/ready/served/paid/cancelled</summary>
        public string Status { get; set; } = OrderStatus.Pending;

        /// <summary>Tổng tiền trước giảm giá</summary>
        public decimal Subtotal { get; set; } = 0;

        /// <summary>Số tiền giảm giá</summary>
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>Thuế VAT</summary>
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>Tổng tiền phải trả</summary>
        public decimal TotalAmount { get; set; } = 0;

        /// <summary>ID voucher áp dụng</summary>
        public Guid? VoucherId { get; set; }

        /// <summary>Số lượng khách</summary>
        public int GuestCount { get; set; } = 1;

        /// <summary>Ghi chú đơn hàng</summary>
        public string? Note { get; set; }

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<TableTransfer> TableTransfers { get; set; } = new List<TableTransfer>();
    }

    /// <summary>Chi tiết từng món trong đơn hàng</summary>
    public class OrderItem : BaseEntity
    {
        /// <summary>Đơn hàng chứa món này</summary>
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        /// <summary>Món ăn được gọi</summary>
        public Guid MenuItemId { get; set; }
        public Menu.Models.MenuItem? MenuItem { get; set; }

        /// <summary>Số lượng</summary>
        public int Quantity { get; set; } = 1;

        /// <summary>Đơn giá gốc tại thời điểm order (chưa giảm giá)</summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>Đơn giá thực tế tại thời điểm order (sau giảm giá, không thay đổi dù menu cập nhật)</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>Thành tiền (quantity * unit_price)</summary>
        public decimal TotalPrice { get; set; }

        /// <summary>Ghi chú riêng cho món (VD: ít đá, không cay)</summary>
        public string? Note { get; set; }

        /// <summary>Trạng thái món trong bếp: pending/cooking/ready/served/cancelled</summary>
        public string Status { get; set; } = OrderItemStatus.Pending;

        /// <summary>Thời điểm gửi vào bếp</summary>
        public DateTime? SentToKitchenAt { get; set; }

        // Navigation
        public ICollection<OrderItemOption> SelectedOptions { get; set; } = new List<OrderItemOption>();
        public KitchenOrder? KitchenOrder { get; set; }
    }

    /// <summary>Option đã chọn cho từng món trong đơn</summary>
    public class OrderItemOption
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Món trong đơn</summary>
        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        /// <summary>Option đã chọn</summary>
        public Guid MenuItemOptionId { get; set; }

        /// <summary>Tên option (snapshot tại thời điểm order)</summary>
        public string OptionName { get; set; } = string.Empty;

        /// <summary>Giá thêm (snapshot)</summary>
        public decimal ExtraPrice { get; set; } = 0;
    }

    /// <summary>Lịch sử chuyển bàn</summary>
    public class TableTransfer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Đơn hàng được chuyển</summary>
        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        /// <summary>Bàn ban đầu</summary>
        public Guid FromTableId { get; set; }

        /// <summary>Bàn đích</summary>
        public Guid ToTableId { get; set; }

        /// <summary>Nhân viên thực hiện chuyển bàn</summary>
        public Guid? TransferredBy { get; set; }

        /// <summary>Thời điểm chuyển bàn</summary>
        public DateTime TransferredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Lý do chuyển bàn</summary>
        public string? Reason { get; set; }
    }

    /// <summary>Ticket bếp (Kitchen Display System)</summary>
    public class KitchenOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Món ăn cần chế biến</summary>
        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        /// <summary>Chi nhánh</summary>
        public Guid BranchId { get; set; }

        /// <summary>Snapshot số bàn để hiển thị nhanh trên KDS</summary>
        public int TableNumber { get; set; }

        /// <summary>Tên món (snapshot)</summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>Số lượng cần làm</summary>
        public int Quantity { get; set; }

        /// <summary>Ghi chú đặc biệt</summary>
        public string? Note { get; set; }

        /// <summary>Số càng cao càng ưu tiên</summary>
        public int Priority { get; set; } = 0;

        /// <summary>Trạng thái: pending/cooking/ready/served</summary>
        public string Status { get; set; } = KitchenOrderStatus.Pending;

        /// <summary>Thời điểm ticket xuất hiện ở bếp</summary>
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Bắt đầu chế biến</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>Hoàn thành chế biến</summary>
        public DateTime? CompletedAt { get; set; }
    }

    public static class OrderStatus
    {
        public const string AwaitingConfirmation = "awaiting_confirmation";
        public const string Pending = "pending";
        public const string Preparing = "preparing";
        public const string Ready = "ready";
        public const string Served = "served";
        public const string Paid = "paid";
        public const string Cancelled = "cancelled";
    }

    public static class OrderItemStatus
    {
        public const string AwaitingConfirmation = "awaiting_confirmation";
        public const string Pending = "pending";
        public const string Cooking = "cooking";
        public const string Ready = "ready";
        public const string Served = "served";
        public const string Cancelled = "cancelled";
    }

    public static class KitchenOrderStatus
    {
        public const string Pending = "pending";
        public const string Cooking = "cooking";
        public const string Ready = "ready";
        public const string Served = "served";
    }
}
