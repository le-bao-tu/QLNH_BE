namespace RestaurantApp.API.Modules.Menu.DTOs
{
    public class CreateMenuCategoryDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public string? BranchIds { get; set; }
    }

    public class UpdateMenuCategoryDto
    {
        public string? Name { get; set; }
        public string? ImageUrl { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public string? BranchIds { get; set; }
    }

    public class MenuCategoryDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string? BranchIds { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<MenuItemDto> Items { get; set; } = new();
    }

    public class CreateMenuItemDto
    {
        public Guid CategoryId { get; set; }
        /// <summary>JSON array of branch Guid strings. NULL = áp dụng tất cả chi nhánh</summary>
        public string? BranchIds { get; set; }
        public Guid? RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Unit { get; set; }
        public int SortOrder { get; set; }
        public string ItemType { get; set; } = "single";
        public List<Guid>? ComboItemIds { get; set; }
    }

    public class UpdateMenuItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Unit { get; set; }
        public bool? IsAvailable { get; set; }
        public int? SortOrder { get; set; }
        public string? ItemType { get; set; }
        public List<Guid>? ComboItemIds { get; set; }
        public string? BranchIds { get; set; }
        public Guid? CategoryId { get; set; }
    }

    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountValue{ get; set; }
        public string? ImageUrl { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int SortOrder { get; set; }
        public string ItemType { get; set; } = "single";
        public string? BranchIds { get; set; }
        public List<MenuComboItemDto>? ComboItems { get; set; }
    }

    public class MenuComboItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
    }
}
