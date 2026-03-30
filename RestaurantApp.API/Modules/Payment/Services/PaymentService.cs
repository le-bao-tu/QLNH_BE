using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Payment.DTOs;
using RestaurantApp.API.Modules.Payment.Models;
using RestaurantApp.API.Modules.Order.Models;

namespace RestaurantApp.API.Modules.Payment.Services
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId);
        Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto, Guid staffId);
        Task<List<PaymentDto>> GetRecentPaymentsAsync(Guid branchId, int take = 20);
    }

    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context) => _context = context;

        public async Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.Order).ThenInclude(o => o!.Table)
                .Where(p => p.OrderId == orderId)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PaymentDto> ProcessPaymentAsync(CreatePaymentDto dto, Guid staffId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null) throw new Exception("Không tìm thấy đơn hàng");
            if (order.Status == OrderStatus.Paid) throw new Exception("Đơn hàng đã được thanh toán");

            var payment = new Models.Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                Method = dto.Method,
                ReferenceCode = dto.ReferenceCode,
                Status = PaymentStatuses.Completed,
                Note = dto.Note,
                CreatedBy = staffId,
                PaidAt = DateTime.UtcNow,
            };

            _context.Payments.Add(payment);

            // Kiểm tra tổng đã thanh toán
            var alreadyPaid = await _context.Payments
                .Where(p => p.OrderId == dto.OrderId && p.Status == PaymentStatuses.Completed)
                .SumAsync(p => p.Amount);

            var totalPaid = alreadyPaid + dto.Amount;

            // Nếu đã thanh toán đủ → đóng đơn, trả bàn
            if (totalPaid >= order.TotalAmount)
            {
                order.Status = OrderStatus.Paid;
                order.UpdatedAt = DateTime.UtcNow;

                if (order.Table != null)
                {
                    order.Table.Status = Table.Models.TableStatus.Available;
                    order.Table.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return ToDto(payment);
        }

        public async Task<List<PaymentDto>> GetRecentPaymentsAsync(Guid branchId, int take = 20)
        {
            return await _context.Payments
                .Include(p => p.Order).ThenInclude(o => o!.Table)
                .Where(p => p.Order!.BranchId == branchId && p.Status == PaymentStatuses.Completed)
                .OrderByDescending(p => p.PaidAt)
                .Take(take)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        private static PaymentDto ToDto(Models.Payment p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            TableNumber = p.Order?.Table?.TableNumber ?? 0,
            Amount = p.Amount,
            Method = p.Method,
            ReferenceCode = p.ReferenceCode,
            Status = p.Status,
            PaidAt = p.PaidAt,
            CreatedAt = p.CreatedAt,
        };
    }
}
