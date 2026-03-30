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
    }
}
