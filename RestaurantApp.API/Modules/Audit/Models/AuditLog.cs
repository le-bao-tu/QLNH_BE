namespace RestaurantApp.API.Modules.Audit.Models
{
    /// <summary>Lịch sử thao tác - ghi lại toàn bộ thay đổi quan trọng</summary>
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Người thực hiện (user_id)</summary>
        public Guid? UserId { get; set; }

        /// <summary>Chi nhánh nhận diện từ người thực hiện</summary>
        public Guid? BranchId { get; set; }

        /// <summary>Nhà hàng nhận diện từ người thực hiện</summary>
        public Guid? RestaurantId { get; set; }

        /// <summary>Hành động: CREATE / UPDATE / DELETE / LOGIN / LOGOUT</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Module liên quan: order / payment / menu / table / reservation...</summary>
        public string Module { get; set; } = string.Empty;

        /// <summary>ID đối tượng bị tác động</summary>
        public Guid? TargetId { get; set; }

        /// <summary>Dữ liệu trước khi thay đổi (JSONB)</summary>
        public string? OldData { get; set; }

        /// <summary>Dữ liệu sau khi thay đổi (JSONB)</summary>
        public string? NewData { get; set; }

        /// <summary>Địa chỉ IP</summary>
        public string? IpAddress { get; set; }

        /// <summary>User Agent (browser)</summary>
        public string? UserAgent { get; set; }

        /// <summary>Thời gian thao tác</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
