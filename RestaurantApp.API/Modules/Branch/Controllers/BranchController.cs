using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Branch.Models;

namespace RestaurantApp.API.Modules.Branch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public BranchController(AppDbContext ctx) => _ctx = ctx;

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            var b = await _ctx.Branches.Where(x => x.RestaurantId == restaurantId).ToListAsync();
            return Ok(b);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBranch(Guid id)
        {
            var b = await _ctx.Branches.FindAsync(id);
            return b == null ? NotFound() : Ok(b);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Models.Branch branch)
        {
            branch.Id = Guid.NewGuid();
            branch.CreatedAt = DateTime.UtcNow;
            _ctx.Branches.Add(branch);
            await _ctx.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBranch), new { id = branch.Id }, branch);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Models.Branch branch)
        {
            var existing = await _ctx.Branches.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = branch.Name;
            existing.Address = branch.Address;
            existing.Phone = branch.Phone;
            existing.IsActive = branch.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
