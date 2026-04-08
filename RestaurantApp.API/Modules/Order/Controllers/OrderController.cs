using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RestaurantApp.API.Modules.Order.DTOs;
using RestaurantApp.API.Modules.Order.Services;

namespace RestaurantApp.API.Modules.Order.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetAll(Guid branchId)
        {
            var orders = await _orderService.GetAllOrders(branchId);
            return Ok(orders);
        }

        [HttpGet("active/branch/{branchId}")]
        public async Task<IActionResult> GetActive(Guid branchId)
        {
            var orders = await _orderService.GetActiveOrdersAsync(branchId);
            return Ok(orders);
        }

        [HttpGet("table/{tableId}")]
        public async Task<IActionResult> GetByTable(Guid tableId)
        {
            var orders = await _orderService.GetByTableAsync(tableId);
            return Ok(orders);
        }

        [HttpGet("kitchen/branch/{branchId}")]
        public async Task<IActionResult> GetKitchenOrders(Guid branchId)
        {
            var orders = await _orderService.GetKitchenOrdersAsync(branchId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var order = await _orderService.CreateAsync(dto, staffId);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(Guid id, [FromBody] AddOrderItemDto dto)
        {
            try
            {
                var order = await _orderService.AddItemAsync(id, dto);
                return order == null ? NotFound() : Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _orderService.UpdateStatusAsync(id, dto.Status);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPatch("items/{itemId}/status")]
        public async Task<IActionResult> UpdateItemStatus(Guid itemId, [FromBody] UpdateOrderItemStatusDto dto)
        {
            var success = await _orderService.UpdateItemStatusAsync(itemId, dto.Status);
            return success ? Ok(new { message = "Đã cập nhật trạng thái món" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _orderService.CancelOrderAsync(id);
            return success ? Ok(new { message = "Đã hủy đơn hàng" }) : NotFound();
        }
    }
}
