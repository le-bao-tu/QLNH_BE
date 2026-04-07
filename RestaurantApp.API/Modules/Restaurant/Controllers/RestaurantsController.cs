using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Restaurant.Models;
using System.IO;

namespace RestaurantApp.API.Modules.Restaurant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RestaurantsController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public RestaurantsController(AppDbContext ctx) => _ctx = ctx;
        
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
            existing.BankId = r.BankId;
            existing.BankNumber = r.BankNumber;
            existing.BankOwner = r.BankOwner;

            await _ctx.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("{id}/upload-logo")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
        {
            var existing = await _ctx.Restaurants.FindAsync(id);
            if (existing == null) return NotFound();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP, GIF)." });

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "restaurants");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/restaurants/{fileName}";

            existing.LogoUrl = imageUrl;

            await _ctx.SaveChangesAsync();
            return Ok(existing);
        }
    }
}
