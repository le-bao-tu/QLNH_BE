using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Inventory.Services;
using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Inventory.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _svc;
        public InventoryController(IInventoryService svc) => _svc = svc;

        // Suppliers
        [HttpGet("suppliers/restaurant/{restaurantId}")]
        public async Task<IActionResult> GetSuppliers(Guid restaurantId, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _svc.GetSuppliersPagedAsync(restaurantId, @params));
            return Ok(await _svc.GetSuppliersAsync(restaurantId));
        }

        [HttpPost("suppliers")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
            => Ok(await _svc.CreateSupplierAsync(dto));

        // Inventory Items
        [HttpGet("items/branch/{branchId}")]
        public async Task<IActionResult> GetItems(Guid branchId, [FromQuery] bool? lowStockOnly, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _svc.GetItemsByBranchPagedAsync(branchId, lowStockOnly, @params));
            return Ok(await _svc.GetItemsByBranchAsync(branchId));
        }

        [HttpGet("items/restaurant/{restaurantId}")]
        public async Task<IActionResult> GetItemsByRestaurant(Guid restaurantId, [FromQuery] bool? lowStockOnly, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _svc.GetItemsByRestaurantPagedAsync(restaurantId, lowStockOnly, @params));
            return Ok(await _svc.GetItemsByRestaurantAsync(restaurantId));
        }

        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItem(Guid id)
        {
            var r = await _svc.GetItemByIdAsync(id); return r == null ? NotFound() : Ok(r);
        }

        [HttpPost("items")]
        public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemDto dto)
            => Ok(await _svc.CreateItemAsync(dto));

        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(Guid id, [FromBody] CreateInventoryItemDto dto)
        {
            var r = await _svc.UpdateItemAsync(id, dto); return r == null ? NotFound() : Ok(r);
        }

        [HttpGet("items/branch/{branchId}/low-stock")]
        public async Task<IActionResult> GetLowStock(Guid branchId)
            => Ok(await _svc.GetLowStockItemsAsync(branchId));

        [HttpGet("items/restaurant/{restaurantId}/low-stock")]
        public async Task<IActionResult> GetLowStockByRestaurant(Guid restaurantId)
            => Ok(await _svc.GetLowStockItemsByRestaurantAsync(restaurantId));

        // Transactions
        [HttpPost("transactions")]
        public async Task<IActionResult> RecordTransaction([FromBody] InventoryTransactionDto dto)
            => Ok(await _svc.RecordTransactionAsync(dto));
    }
}
