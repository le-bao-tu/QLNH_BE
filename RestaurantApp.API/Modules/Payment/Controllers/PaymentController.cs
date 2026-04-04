using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Payment.Models;

namespace RestaurantApp.API.Modules.Payment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        public PaymentController(AppDbContext ctx) => _ctx = ctx;

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var p = await _ctx.Payments.Where(x => x.OrderId == orderId).ToListAsync();
            return Ok(p);
        }

        [HttpGet("recent/branch/{branchId}")]
        public async Task<IActionResult> GetRecent(Guid branchId)
        {
            var p = await _ctx.Payments
                .Include(x => x.Order)
                .Where(x => x.Order.BranchId == branchId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToListAsync();
            return Ok(p);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(Models.Payment payment)
        {
            var order = await _ctx.Orders.FindAsync(payment.OrderId);
            if (order == null) return NotFound("Order not found");

            payment.Id = Guid.NewGuid();
            payment.CreatedAt = DateTime.UtcNow;
            payment.Status = "completed"; // Mock success for now
            payment.PaidAt = DateTime.UtcNow;

            _ctx.Payments.Add(payment);

            // Update order status if total payment covers the order
            var totalPaid = await _ctx.Payments.Where(p => p.OrderId == payment.OrderId && p.Status == "completed").SumAsync(p => p.Amount) + payment.Amount;
            
            if (totalPaid >= order.TotalAmount)
            {
                order.Status = "paid";
                
                // Update table status back to available
                var table = await _ctx.Tables.FindAsync(order.TableId);
                if (table != null) table.Status = "available";
            }

            await _ctx.SaveChangesAsync();
            return Ok(payment);
        }
    }
}
