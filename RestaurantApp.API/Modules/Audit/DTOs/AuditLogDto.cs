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

    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? RestaurantId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public Guid? TargetId { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

