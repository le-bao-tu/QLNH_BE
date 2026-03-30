using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Branch.Models
{
    /// <summary>Chi nhánh của nhà hàng</summary>
    public class Branch : BaseEntity
    {
        /// <summary>Nhà hàng sở hữu chi nhánh</summary>
        public Guid RestaurantId { get; set; }
        public Restaurant.Models.Restaurant? Restaurant { get; set; }

        /// <summary>Tên chi nhánh</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Địa chỉ chi nhánh</summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>Số điện thoại chi nhánh</summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>Trạng thái hoạt động</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Danh sách bàn trong chi nhánh</summary>
        public ICollection<Table.Models.Table> Tables { get; set; } = new List<Table.Models.Table>();
    }
}
