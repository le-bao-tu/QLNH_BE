using System;

namespace RestaurantApp.API.Modules.Audit.DTOs
{
    public class AuditLogCreateDto
    {
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public Guid? TargetId { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
    }
}
