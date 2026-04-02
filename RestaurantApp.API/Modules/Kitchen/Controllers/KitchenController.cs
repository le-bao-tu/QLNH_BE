using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Order.Models;

namespace RestaurantApp.API.Modules.Kitchen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KitchenController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public KitchenController(AppDbContext ctx) => _ctx = ctx;

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetByBranch(Guid branchId)
        {
            var tickets = await _ctx.KitchenOrders
                .Include(ko => ko.OrderItem)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(ko => ko.BranchId == branchId && ko.Status != KitchenOrderStatus.Served)
                .OrderBy(ko => ko.ReceivedAt)
                .Select(ko => new {
                    id = ko.Id,
                    orderItemId = ko.OrderItemId,
                    branchId = ko.BranchId,
                    tableNumber = ko.TableNumber,
                    itemName = ko.OrderItem.MenuItem.Name,
                    quantity = ko.OrderItem.Quantity,
                    note = ko.OrderItem.Note,
                    priority = ko.Priority,
                    status = ko.Status,
                    receivedAt = ko.ReceivedAt,
                    startedAt = ko.StartedAt,
                    completedAt = ko.CompletedAt
                })
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusUpdateDto dto)
        {
            var ko = await _ctx.KitchenOrders.FindAsync(id);
            if (ko == null) return NotFound();

            ko.Status = dto.Status;
            
            if (dto.Status == KitchenOrderStatus.Cooking) ko.StartedAt = DateTime.UtcNow;
            if (dto.Status == KitchenOrderStatus.Ready) ko.CompletedAt = DateTime.UtcNow;
            
            // Also update the OrderItem status
            var orderItem = await _ctx.OrderItems.FindAsync(ko.OrderItemId);
            if (orderItem != null)
            {
                if (dto.Status == KitchenOrderStatus.Cooking) orderItem.Status = OrderItemStatus.Pending;
                if (dto.Status == KitchenOrderStatus.Ready) orderItem.Status = OrderItemStatus.Ready;
                if (dto.Status == KitchenOrderStatus.Served) orderItem.Status = OrderItemStatus.Served;
            }

            await _ctx.SaveChangesAsync();
            return Ok();
        }

        public class StatusUpdateDto { public string Status { get; set; } = ""; }
    }
}
