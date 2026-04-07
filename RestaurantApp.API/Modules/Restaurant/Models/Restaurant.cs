using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Restaurant.Models
{
    /// <summary>Thông tin nhà hàng</summary>
    public class Restaurant : BaseEntity
    {
        /// <summary>Tên nhà hàng</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>ID chủ nhà hàng</summary>
        public Guid OwnerId { get; set; }

        /// <summary>Đường dẫn logo nhà hàng</summary>
        public string? LogoUrl { get; set; }

        /// <summary>Mã số thuế doanh nghiệp</summary>
        public string? TaxCode { get; set; }

        /// <summary>Địa chỉ website</summary>
        public string? Website { get; set; }

        /// <summary>Số điện thoại nhà hàng</summary>
        public string? Phone { get; set; }

        /// <summary>Mô tả nhà hàng</summary>
        public string? Description { get; set; }

        /// <summary>Địa chỉ trụ sở chính</summary>
        public string? Address { get; set; }

        /// <summary>Email liên hệ</summary>
        public string? Email { get; set; }

        /// <summary> Tên ngân hàng </summary>
        public string? BankId { get; set; }

        /// <summary> Số tài khoản </summary>
        public string? BankNumber { get; set; }

        /// <summary> Chủ tài khoản </summary>
        public string? BankOwner { get; set; }

        // Navigation
        public ICollection<Branch.Models.Branch> Branches { get; set; } = new List<Branch.Models.Branch>();
    }
}
