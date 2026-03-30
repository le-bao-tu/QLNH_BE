using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Promotion.Models
{
    /// <summary>Chương trình khuyến mãi</summary>
    public class Promotion : BaseEntity
    {
        /// <summary>Nhà hàng áp dụng</summary>
        public Guid RestaurantId { get; set; }

        /// <summary>Tên chương trình khuyến mãi</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả chi tiết</summary>
        public string? Description { get; set; }

        /// <summary>Loại: percent_discount / fixed_discount / free_item / buy_x_get_y / loyalty_redeem</summary>
        public string Type { get; set; } = PromotionType.PercentDiscount;

        /// <summary>Giá trị giảm (% hoặc số tiền cố định)</summary>
        public decimal DiscountValue { get; set; }

        /// <summary>Giá trị đơn hàng tối thiểu để áp dụng</summary>
        public decimal MinOrderAmount { get; set; } = 0;

        /// <summary>Giảm tối đa (áp dụng với loại %)</summary>
        public decimal? MaxDiscount { get; set; }

        /// <summary>Ngày bắt đầu áp dụng</summary>
        public DateOnly StartDate { get; set; }

        /// <summary>Ngày kết thúc</summary>
        public DateOnly EndDate { get; set; }

        /// <summary>Đang kích hoạt</summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<VoucherCode> VoucherCodes { get; set; } = new List<VoucherCode>();
    }

    /// <summary>Mã voucher cho chương trình khuyến mãi</summary>
    public class VoucherCode : BaseEntity
    {
        /// <summary>Chương trình khuyến mãi liên kết</summary>
        public Guid PromotionId { get; set; }
        public Promotion? Promotion { get; set; }

        /// <summary>Mã voucher (unique)</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Số lần sử dụng tối đa (0 = unlimited)</summary>
        public int UsageLimit { get; set; } = 1;

        /// <summary>Đã sử dụng bao nhiêu lần</summary>
        public int UsedCount { get; set; } = 0;

        /// <summary>Gán riêng cho khách hàng (nullable = dùng chung)</summary>
        public Guid? AssignedTo { get; set; }

        /// <summary>Hạn sử dụng</summary>
        public DateTime? ExpiresAt { get; set; }
    }

    public static class PromotionType
    {
        public const string PercentDiscount = "percent_discount";
        public const string FixedDiscount = "fixed_discount";
        public const string FreeItem = "free_item";
        public const string BuyXGetY = "buy_x_get_y";
        public const string LoyaltyRedeem = "loyalty_redeem";
    }
}
