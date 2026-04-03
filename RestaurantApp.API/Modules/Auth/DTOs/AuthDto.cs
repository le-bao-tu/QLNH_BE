namespace RestaurantApp.API.Modules.Auth.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RestaurantName { get; set; } = string.Empty;
    }

    /// <summary>Owner tạo tài khoản cho nhân viên/quản lý</summary>
    public class CreateStaffDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        /// <summary>owner | manager | cashier | waiter | chef | bartender</summary>
        public string Role { get; set; } = "waiter";
        public Guid RestaurantId { get; set; }
        public Guid BranchId { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? RestaurantId { get; set; }
        public Guid? BranchId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
