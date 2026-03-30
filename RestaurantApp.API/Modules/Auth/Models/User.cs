using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Auth.Models
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Admin, Manager, Staff, User
    }
}
