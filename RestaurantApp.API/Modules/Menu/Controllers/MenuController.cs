using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Menu.DTOs;
using RestaurantApp.API.Modules.Menu.Services;

namespace RestaurantApp.API.Modules.Menu.Controllers
{
    [ApiController]
    [Route("api/menu")]
    [Authorize]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // ===== CATEGORIES =====
        [HttpGet("categories/restaurant/{restaurantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories(Guid restaurantId)
        {
            var cats = await _menuService.GetCategoriesByRestaurantAsync(restaurantId);
            return Ok(cats);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateMenuCategoryDto dto)
        {
            try
            {
                var cat = await _menuService.CreateCategoryAsync(dto);
                return Ok(cat);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateMenuCategoryDto dto)
        {
            var cat = await _menuService.UpdateCategoryAsync(id, dto);
            return cat == null ? NotFound() : Ok(cat);
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var success = await _menuService.DeleteCategoryAsync(id);
            return success ? Ok(new { message = "Đã xóa danh mục" }) : NotFound();
        }

        // ===== ITEMS =====
        [HttpGet("items/restaurant/{restaurantId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllItems(Guid restaurantId)
        {
            var items = await _menuService.GetAllItemsByRestaurantAsync(restaurantId);
            return Ok(items);
        }

        [HttpGet("items/branch/{branchId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItemsByBranch(Guid branchId)
        {
            var items = await _menuService.GetAllItemsByBranchAsync(branchId);
            return Ok(items);
        }

        [HttpGet("items/category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItemsByCategory(Guid categoryId)
        {
            var items = await _menuService.GetItemsByCategoryAsync(categoryId);
            return Ok(items);
        }

        [HttpGet("items/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetItem(Guid id)
        {
            var item = await _menuService.GetItemByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] CreateMenuItemDto dto)
        {
            try
            {
                var item = await _menuService.CreateItemAsync(dto);
                return Ok(item);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateMenuItemDto dto)
        {
            var item = await _menuService.UpdateItemAsync(id, dto);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var success = await _menuService.DeleteItemAsync(id);
            return success ? Ok(new { message = "Đã xóa món" }) : NotFound();
        }

        // ===== UPLOAD ẢNH MÓN ĂN =====
        [HttpPost("items/upload-image")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<IActionResult> UploadMenuImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file ảnh." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPEG, PNG, WEBP, GIF)." });

            // Tạo thư mục lưu ảnh
            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "menu");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Tạo tên file duy nhất
            var ext = Path.GetExtension(file.FileName).ToLower();
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lấy base URL từ request
            var imageUrl = $"/uploads/menu/{fileName}";

            return Ok(new { url = imageUrl });
        }
    }
}
