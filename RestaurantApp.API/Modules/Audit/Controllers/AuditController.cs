using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Audit.Models;
using RestaurantApp.API.Modules.Audit.DTOs;
using RestaurantApp.API.Common;
using System.Security.Claims;

namespace RestaurantApp.API.Modules.Audit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditController(AppDbContext ctx, IHttpContextAccessor httpContextAccessor)
        {
            _ctx = ctx;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] PaginationParams @params, [FromQuery] Guid? branchId = null, [FromQuery] string? module = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userBranchIdStr = User.FindFirst("branchId")?.Value;
            var restaurantIdStr = User.FindFirst("restaurantId")?.Value;
            
            Guid? userBranchId = Guid.TryParse(userBranchIdStr, out var b) ? b : null;
            Guid? restaurantId = Guid.TryParse(restaurantIdStr, out var r) ? r : null;

            var query = _ctx.AuditLogs.AsQueryable();

            if (restaurantId.HasValue)
                query = query.Where(l => l.RestaurantId == restaurantId.Value);

            if (userRole?.ToLower() != "owner")
                query = query.Where(l => l.BranchId == userBranchId);
            else if (branchId.HasValue)
                query = query.Where(l => l.BranchId == branchId.Value);

            if (!string.IsNullOrEmpty(module))
                query = query.Where(l => l.Module == module);

            if (!string.IsNullOrEmpty(@params.Search))
                query = query.Where(l => l.Action.Contains(@params.Search) || l.Module.Contains(@params.Search));

            var result = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new AuditLogDto {
                    Id = l.Id,
                    UserId = l.UserId,
                    BranchId = l.BranchId,
                    RestaurantId = l.RestaurantId,
                    UserName = _ctx.Users.Where(u => u.Id == l.UserId).Select(u => u.FullName).FirstOrDefault() ?? "Hệ thống",
                    Action = l.Action,
                    Module = l.Module,
                    TargetId = l.TargetId,
                    OldData = l.OldData,
                    NewData = l.NewData,
                    IpAddress = l.IpAddress,
                    UserAgent = l.UserAgent,
                    CreatedAt = l.CreatedAt
                })
                .ToPagedResultAsync(@params.PageIndex, @params.PageSize);

            return Ok(result);
        }

        [HttpPost("log")]
        public async Task<IActionResult> CreateLog([FromBody] AuditLogCreateDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("nameid")?.Value;
            var branchIdStr = User.FindFirst("branchId")?.Value;
            var restaurantIdStr = User.FindFirst("restaurantId")?.Value;
            
            Guid? userId = Guid.TryParse(userIdStr, out var g) ? g : null;
            Guid? branchId = Guid.TryParse(branchIdStr, out var b) ? b : null;
            Guid? restaurantId = Guid.TryParse(restaurantIdStr, out var r) ? r : null;

            var log = new AuditLog
            {
                UserId = userId,
                BranchId = branchId,
                RestaurantId = restaurantId,
                Action = dto.Action,
                Module = dto.Module,
                TargetId = dto.TargetId,
                OldData = dto.OldData,
                NewData = dto.NewData,
                IpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _ctx.AuditLogs.Add(log);
            await _ctx.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
