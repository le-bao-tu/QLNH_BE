using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Restaurant.Models;

namespace RestaurantApp.API.Modules.Restaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public RestaurantController(AppDbContext ctx) => _ctx = ctx;
        
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var r = await _ctx.Restaurants.Include(x => x.Branches).FirstOrDefaultAsync(x => x.Id == id);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Models.Restaurant r)
        {
            var existing = await _ctx.Restaurants.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = r.Name;
            existing.LogoUrl = r.LogoUrl;
            existing.TaxCode = r.TaxCode;
            existing.Website = r.Website;
            existing.Phone = r.Phone;
            existing.Email = r.Email;
            existing.Address = r.Address;
            existing.Description = r.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
