using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Inventory.Models
{
    /// <summary>Nhà cung cấp nguyên liệu</summary>
    public class Supplier : BaseEntity
    {
        /// <summary>Nhà hàng quản lý nhà cung cấp</summary>
        public Guid RestaurantId { get; set; }

        /// <summary>Tên nhà cung cấp</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Tên người liên hệ</summary>
        public string? ContactName { get; set; }

        /// <summary>Số điện thoại liên hệ</summary>
        public string? Phone { get; set; }

        /// <summary>Email liên hệ</summary>
        public string? Email { get; set; }

        /// <summary>Địa chỉ nhà cung cấp</summary>
        public string? Address { get; set; }

        /// <summary>Ghi chú</summary>
        public string? Note { get; set; }

        // Navigation
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

    /// <summary>Nguyên liệu trong kho</summary>
    public class InventoryItem : BaseEntity
    {
        /// <summary>Chi nhánh quản lý kho</summary>
        public Guid BranchId { get; set; }
        public Branch.Models.Branch? Branch { get; set; }

        /// <summary>Nhà cung cấp chính</summary>
        public Guid? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        /// <summary>Tên nguyên liệu</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Đơn vị tính (kg, lít, cái, hộp...)</summary>
        public string Unit { get; set; } = "kg";

        /// <summary>Số lượng tồn kho hiện tại</summary>
        public decimal CurrentQuantity { get; set; } = 0;

        /// <summary>Mức cảnh báo tồn kho thấp</summary>
        public decimal MinQuantity { get; set; } = 0;

        /// <summary>Giá nhập gần nhất</summary>
        public decimal CostPrice { get; set; } = 0;

        // Navigation
        public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<MenuItemRecipe> Recipes { get; set; } = new List<MenuItemRecipe>();
    }

    /// <summary>Giao dịch xuất/nhập kho</summary>
    public class InventoryTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nguyên liệu liên quan</summary>
        public Guid InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }

        /// <summary>Loại giao dịch: import / export / adjust / waste</summary>
        public string Type { get; set; } = "import";

        /// <summary>Số lượng thay đổi (dương = nhập, âm = xuất)</summary>
        public decimal QuantityChange { get; set; }

        /// <summary>Tồn kho sau giao dịch</summary>
        public decimal QuantityAfter { get; set; }

        /// <summary>ID đơn liên quan (order/purchase)</summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>Ghi chú giao dịch</summary>
        public string? Note { get; set; }

        /// <summary>Nhân viên thực hiện</summary>
        public Guid? PerformedBy { get; set; }

        /// <summary>Thời gian giao dịch</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Công thức món ăn - liên kết món với nguyên liệu</summary>
    public class MenuItemRecipe
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Món ăn sử dụng nguyên liệu này</summary>
        public Guid MenuItemId { get; set; }

        /// <summary>Nguyên liệu cần dùng</summary>
        public Guid InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }

        /// <summary>Lượng nguyên liệu mỗi suất</summary>
        public decimal QuantityUsed { get; set; }

        /// <summary>Đơn vị</summary>
        public string Unit { get; set; } = "g";
    }
}
