using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Audit.Models;

namespace RestaurantApp.API.Modules.Audit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public AuditController(AppDbContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? module = null, [FromQuery] string? search = null)
        {
            var query = _ctx.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(module))
                query = query.Where(l => l.Module == module);

            // Join with Users to get usernames
            var result = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new {
                    l.Id,
                    l.UserId,
                    UserName = _ctx.Users.Where(u => u.Id == l.UserId).Select(u => u.FullName).FirstOrDefault() ?? "System",
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
    }
}
