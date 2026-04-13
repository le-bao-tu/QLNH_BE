using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Auth.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsOwner { get; set; }
        public Guid RoleId { get; set; } // For Role
        public string? RoleName { get; set; }
        public Guid? RestaurantId { get; set; } // For Owner scoping
        public Guid? BranchId { get; set; } // For Staff/Manager scoping
    }
}
