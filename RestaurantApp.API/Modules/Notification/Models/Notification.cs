namespace RestaurantApp.API.Modules.Notification.Models
{
    /// <summary>Thông báo gửi đến người dùng</summary>
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Người nhận (user_id)</summary>
        public Guid RecipientId { get; set; }

        /// <summary>Loại: order_new / order_ready / inventory_low / reservation_new / payment_completed</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>Tiêu đề thông báo</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Nội dung thông báo</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>ID đối tượng liên quan (orderId, reservationId...)</summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>Đã đọc chưa</summary>
        public bool IsRead { get; set; } = false;

        /// <summary>Thời gian tạo</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public static class Types
        {
            public const string OrderNew = "order_new";
            public const string OrderReady = "order_ready";
            public const string InventoryLow = "inventory_low";
            public const string ReservationNew = "reservation_new";
            public const string PaymentCompleted = "payment_completed";
        }
    }
}
