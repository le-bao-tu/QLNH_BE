using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RestaurantApp.API.Modules.Order.DTOs;
using RestaurantApp.API.Modules.Order.Services;
using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Order.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [AllowAnonymous]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("userId")?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetOrdersByBranch(Guid branchId, [FromQuery] string? status, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _orderService.GetAllOrdersPaged(branchId, status, @params));
            var orders = await _orderService.GetAllOrders(branchId);
            return Ok(orders);
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetOrdersByRestaurent(Guid restaurantId, [FromQuery] string? status, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _orderService.GetOrdersByRestaurantPaged(restaurantId, status, @params));
            var orders = await _orderService.GetOrdersByRestaurant(restaurantId);
            return Ok(orders);
        }

        [HttpGet("active/branch/{branchId}")]
        public async Task<IActionResult> GetActive(Guid branchId)
        {
            var orders = await _orderService.GetActiveOrdersAsync(branchId);
            return Ok(orders);
        }

        [HttpGet("active/restaurant/{restaurantId}")]
        public async Task<IActionResult> GetActiveInRestaurant(Guid restaurantId)
        {
            var orders = await _orderService.GetActiveOrdersInRestaurantAsync(restaurantId);
            return Ok(orders);
        }

        [HttpGet("table/{tableId}")]
        [AllowAnonymous]
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

        [HttpGet("stats/dish-sales/branch/{branchId}")]
        public async Task<IActionResult> GetDishSalesStats(Guid branchId, [FromQuery] string filter = "today")
        {
            var stats = await _orderService.GetDishSalesStatsAsync(branchId, filter);
            return Ok(stats);
        }

        [HttpGet("stats/dish-sales/restaurant/{restaurantId}")]
        public async Task<IActionResult> GetDishSalesStatsInRestaurant(Guid restaurantId, [FromQuery] string filter = "today")
        {
            var stats = await _orderService.GetDishSalesStatsInRestaurantAsync(restaurantId, filter);
            return Ok(stats);
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
        [AllowAnonymous]
        public async Task<IActionResult> AddItem(Guid id, [FromBody] AddOrderItemDto dto)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var order = await _orderService.AddItemAsync(id, dto, staffId);
                return order == null ? NotFound() : Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("{id}/items/multiple")]
        [AllowAnonymous]
        public async Task<IActionResult> AddItems(Guid id, [FromBody] List<AddOrderItemDto> dtos)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var order = await _orderService.AddItemsAsync(id, dtos, staffId);
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
        
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            var order = await _orderService.ConfirmOrderAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost("items/{itemId}/confirm")]
        public async Task<IActionResult> ConfirmItem(Guid itemId)
        {
            var success = await _orderService.ConfirmItemAsync(itemId);
            return success ? Ok(new { message = "Đã xác nhận món ăn" }) : NotFound();
        }

        [HttpPost("{id}/send-to-kitchen")]
        public async Task<IActionResult> SendToKitchen(Guid id)
        {
            var success = await _orderService.SendToKitchenAsync(id);
            return success ? Ok(new { message = "Đã gửi yêu cầu vào bếp" }) : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _orderService.CancelOrderAsync(id);
            return success ? Ok(new { message = "Đã hủy đơn hàng" }) : NotFound();
        }

        [HttpGet("pending/table/{tableId}")]
        public async Task<IActionResult> GetPendingOrderInTable(Guid tableId)
        {
            var orders = await _orderService.GetPendingOrderInTableAsync(tableId);
            return Ok(orders);
        }

        [HttpPost("split")]
        public async Task<IActionResult> SplitTable([FromBody] SplitTableDto dto)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var order = await _orderService.SplitTableAsync(dto, staffId);
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPost("merge")]
        public async Task<IActionResult> MergeTables([FromBody] MergeTablesDto dto)
        {
            try
            {
                var staffId = GetCurrentUserId();
                var order = await _orderService.MergeTablesAsync(dto, staffId);
                return Ok(order);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }
    }
}
