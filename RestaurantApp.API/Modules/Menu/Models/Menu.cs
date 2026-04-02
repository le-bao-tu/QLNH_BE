using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Menu.Models
{
    /// <summary>Danh mục món ăn (Khai vị, Món chính, Tráng miệng...)</summary>
    public class MenuCategory : BaseEntity
    {
        /// <summary>Nhà hàng sở hữu danh mục</summary>
        public Guid RestaurantId { get; set; }

        /// <summary>Tên danh mục</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Hình ảnh danh mục</summary>
        public string? ImageUrl { get; set; }

        /// <summary>Thứ tự hiển thị trên menu</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>Danh mục đang hoạt động hay ẩn</summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }

    /// <summary>Thông tin chi tiết từng món ăn</summary>
    public class MenuItem : BaseEntity
    {
        /// <summary>Danh mục menu</summary>
        public Guid CategoryId { get; set; }
        public MenuCategory? Category { get; set; }

        /// <summary>NULL = áp dụng cho tất cả chi nhánh</summary>
        public Guid? BranchId { get; set; }

        /// <summary>Tên món ăn</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Mô tả món ăn</summary>
        public string? Description { get; set; }

        /// <summary>Giá gốc (base price)</summary>
        public decimal BasePrice { get; set; }

        /// <summary>Đơn vị tính: phần, ly, đĩa...</summary>
        public string Unit { get; set; } = "phần";

        /// <summary>Hình ảnh món ăn</summary>
        public string? ImageUrl { get; set; }

        /// <summary>FALSE khi món tạm hết hoặc không phục vụ</summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>Thứ tự hiển thị trong danh mục</summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>Loại món ăn: single / combo</summary>
        public string ItemType { get; set; } = "single";

        // Navigation
        public ICollection<MenuItemOptionGroup> OptionGroups { get; set; } = new List<MenuItemOptionGroup>();
        public ICollection<Inventory.Models.MenuItemRecipe> Recipes { get; set; } = new List<Inventory.Models.MenuItemRecipe>();
        public ICollection<MenuItemCombo> ComboItems { get; set; } = new List<MenuItemCombo>();
    }

    /// <summary>Nhóm option của món (VD: Kích thước, Topping, Mức độ chín...)</summary>
    public class MenuItemOptionGroup : BaseEntity
    {
        /// <summary>Món ăn sở hữu nhóm option</summary>
        public Guid MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

        /// <summary>Tên nhóm (VD: "Kích thước", "Topping")</summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>TRUE = khách bắt buộc phải chọn một option</summary>
        public bool IsRequired { get; set; } = false;

        /// <summary>TRUE = khách được chọn nhiều option cùng lúc</summary>
        public bool IsMultiple { get; set; } = false;

        // Navigation
        public ICollection<MenuItemOption> Options { get; set; } = new List<MenuItemOption>();
    }

    /// <summary>Option cụ thể trong nhóm (VD: "Lớn", "Ít đường")</summary>
    public class MenuItemOption : BaseEntity
    {
        /// <summary>Nhóm option chứa option này</summary>
        public Guid OptionGroupId { get; set; }
        public MenuItemOptionGroup? OptionGroup { get; set; }

        /// <summary>Tên lựa chọn (VD: "Lớn", "Không đá")</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Giá cộng thêm so với giá gốc của món</summary>
        public decimal ExtraPrice { get; set; } = 0;

        /// <summary>Được chọn mặc định</summary>
        public bool IsDefault { get; set; } = false;
    }

    /// <summary>Chi tiết món ăn cấu thành một combo</summary>
    public class MenuItemCombo : BaseEntity
    {
        /// <summary>Món combo</summary>
        public Guid ComboItemId { get; set; }
        public MenuItem? ComboItem { get; set; }

        /// <summary>Món đơn lẻ trong combo</summary>
        public Guid SingleItemId { get; set; }
        public MenuItem? SingleItem { get; set; }

        /// <summary>Số lượng món đơn lẻ trong combo (thường là 1)</summary>
        public int Quantity { get; set; } = 1;
    }
}
