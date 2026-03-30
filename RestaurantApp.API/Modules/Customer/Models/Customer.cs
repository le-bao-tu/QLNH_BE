using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Customer.Models
{
    /// <summary>Thông tin khách hàng</summary>
    public class Customer : BaseEntity
    {
        /// <summary>Nhà hàng sở hữu dữ liệu khách hàng</summary>
        public Guid RestaurantId { get; set; }

        /// <summary>Họ tên đầy đủ</summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>Số điện thoại (unique trong nhà hàng)</summary>
        public string? Phone { get; set; }

        /// <summary>Email liên hệ</summary>
        public string? Email { get; set; }

        /// <summary>Ngày sinh</summary>
        public DateOnly? DateOfBirth { get; set; }

        /// <summary>Giới tính: male / female / other</summary>
        public string? Gender { get; set; }

        /// <summary>Điểm tích lũy hiện tại</summary>
        public int LoyaltyPoints { get; set; } = 0;

        /// <summary>Hạng thành viên: bronze / silver / gold / platinum</summary>
        public string LoyaltyTier { get; set; } = LoyaltyTiers.Bronze;

        /// <summary>Ghi chú về khách hàng</summary>
        public string? Note { get; set; }

        // Navigation
        public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    }

    /// <summary>Lịch sử cộng/trừ điểm tích lũy</summary>
    public class LoyaltyTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Khách hàng sở hữu giao dịch điểm</summary>
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        /// <summary>Đơn hàng liên quan (nếu có)</summary>
        public Guid? OrderId { get; set; }

        /// <summary>Số điểm thay đổi (dương = cộng, âm = trừ)</summary>
        public int PointsChange { get; set; }

        /// <summary>Lý do: earn / redeem / adjust / birthday / promotion</summary>
        public string Reason { get; set; } = "earn";

        /// <summary>Mô tả chi tiết</summary>
        public string? Description { get; set; }

        /// <summary>Điểm còn lại sau giao dịch</summary>
        public int BalanceAfter { get; set; }

        /// <summary>Thời gian giao dịch</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class LoyaltyTiers
    {
        public const string Bronze = "bronze";
        public const string Silver = "silver";
        public const string Gold = "gold";
        public const string Platinum = "platinum";
    }
}
