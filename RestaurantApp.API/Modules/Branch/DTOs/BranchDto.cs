namespace RestaurantApp.API.Modules.Branch.DTOs
{
    public class CreateBranchDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateBranchDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
    }

    public class BranchDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TableCount { get; set; }
    }
}
