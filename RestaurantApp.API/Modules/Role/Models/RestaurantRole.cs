using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Role.Models
{
    public class RestaurantRole : BaseEntity
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        /// <summary>JSON array of strings for permissions</summary>
        public string Permissions { get; set; } = "[]";
    }
}
