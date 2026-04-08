using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Audit.Models;
using RestaurantApp.API.Modules.Audit.DTOs;
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
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? module = null, [FromQuery] string? search = null, [FromQuery] Guid? branchId = null)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userBranchIdStr = User.FindFirst("branchId")?.Value;
            var restaurantIdStr = User.FindFirst("restaurantId")?.Value;
            
            Guid? userBranchId = Guid.TryParse(userBranchIdStr, out var b) ? b : null;
            Guid? restaurantId = Guid.TryParse(restaurantIdStr, out var r) ? r : null;

            var query = _ctx.AuditLogs.AsQueryable();

            // Luôn lọc theo RestaurantId để đảm bảo an toàn dữ liệu
            if (restaurantId.HasValue)
            {
                query = query.Where(l => l.RestaurantId == restaurantId.Value);
            }

            // Phân quyền theo Role và lọc theo BranchId yêu cầu
            if (userRole?.ToLower() != "owner")
            {
                // Manager/Staff chỉ xem được chi nhánh của mình
                query = query.Where(l => l.BranchId == userBranchId);
            }
            else if (branchId.HasValue)
            {
                // Owner có thể xem tất cả, hoặc lọc theo chi nhánh cụ thể nếu chọn
                query = query.Where(l => l.BranchId == branchId.Value);
            }

            if (!string.IsNullOrEmpty(module))
                query = query.Where(l => l.Module == module);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(l => l.Action.Contains(search) || l.Module.Contains(search));

            var result = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new {
                    l.Id,
                    l.UserId,
                    l.BranchId,
                    l.RestaurantId,
                    UserName = _ctx.Users.Where(u => u.Id == l.UserId).Select(u => u.FullName).FirstOrDefault() ?? "Hệ thống",
                    l.Action,
                    l.Module,
                    l.TargetId,
                    l.OldData,
                    l.NewData,
                    l.IpAddress,
                    l.UserAgent,
                    l.CreatedAt
                })
                .ToListAsync();

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
