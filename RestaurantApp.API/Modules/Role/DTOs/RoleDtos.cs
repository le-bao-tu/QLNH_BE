using System.ComponentModel.DataAnnotations;

namespace RestaurantApp.API.Modules.Role.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateRoleDto
    {
        [Required]
        public Guid RestaurantId { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRoleDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public List<string> Permissions { get; set; } = new();
    }
}
