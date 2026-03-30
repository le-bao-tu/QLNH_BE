using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Customer.Models;

namespace RestaurantApp.API.Modules.Customer.Services
{
    public record CustomerDto(Guid Id, Guid RestaurantId, string FullName, string? Phone,
        string? Email, int LoyaltyPoints, string LoyaltyTier, string? Note, DateTime CreatedAt);

    public record CreateCustomerDto(Guid RestaurantId, string FullName, string? Phone,
        string? Email, string? Gender, string? Note);

    public record LoyaltyTxDto(Guid Id, Guid CustomerId, int PointsChange, string Reason,
        string? Description, int BalanceAfter, DateTime CreatedAt);

    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetByRestaurantAsync(Guid restaurantId, string? search = null);
        Task<CustomerDto?> GetByIdAsync(Guid id);
        Task<CustomerDto?> GetByPhoneAsync(Guid restaurantId, string phone);
        Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
        Task<CustomerDto?> UpdateAsync(Guid id, CreateCustomerDto dto);
        Task<List<LoyaltyTxDto>> GetLoyaltyHistoryAsync(Guid customerId);
        Task<LoyaltyTxDto> AddLoyaltyPointsAsync(Guid customerId, int points, string reason, string? description, Guid? orderId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _ctx;
        public CustomerService(AppDbContext ctx) => _ctx = ctx;

        public async Task<List<CustomerDto>> GetByRestaurantAsync(Guid restaurantId, string? search = null)
        {
            var query = _ctx.Customers.Where(c => c.RestaurantId == restaurantId);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.FullName.Contains(search) || (c.Phone != null && c.Phone.Contains(search)));
            return await query.OrderByDescending(c => c.CreatedAt).Select(c => ToDto(c)).ToListAsync();
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id)
        {
            var c = await _ctx.Customers.FindAsync(id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CustomerDto?> GetByPhoneAsync(Guid restaurantId, string phone)
        {
            var c = await _ctx.Customers.FirstOrDefaultAsync(c => c.RestaurantId == restaurantId && c.Phone == phone);
            return c == null ? null : ToDto(c);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
        {
            var c = new Models.Customer
            {
                RestaurantId = dto.RestaurantId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                Email = dto.Email,
                Gender = dto.Gender,
                Note = dto.Note,
                LoyaltyTier = LoyaltyTiers.Bronze,
            };
            _ctx.Customers.Add(c);
            await _ctx.SaveChangesAsync();
            return ToDto(c);
        }

        public async Task<CustomerDto?> UpdateAsync(Guid id, CreateCustomerDto dto)
        {
            var c = await _ctx.Customers.FindAsync(id);
            if (c == null) return null;
            c.FullName = dto.FullName;
            c.Phone = dto.Phone;
            c.Email = dto.Email;
            c.Gender = dto.Gender;
            c.Note = dto.Note;
            c.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
            return ToDto(c);
        }

        public async Task<List<LoyaltyTxDto>> GetLoyaltyHistoryAsync(Guid customerId)
            => await _ctx.LoyaltyTransactions
                .Where(lt => lt.CustomerId == customerId)
                .OrderByDescending(lt => lt.CreatedAt)
                .Select(lt => new LoyaltyTxDto(lt.Id, lt.CustomerId, lt.PointsChange, lt.Reason, lt.Description, lt.BalanceAfter, lt.CreatedAt))
                .ToListAsync();

        public async Task<LoyaltyTxDto> AddLoyaltyPointsAsync(Guid customerId, int points, string reason, string? description, Guid? orderId)
        {
            var customer = await _ctx.Customers.FindAsync(customerId) ?? throw new Exception("Không tìm thấy khách hàng");
            customer.LoyaltyPoints += points;

            // Update tier
            customer.LoyaltyTier = customer.LoyaltyPoints switch
            {
                >= 10000 => LoyaltyTiers.Platinum,
                >= 5000 => LoyaltyTiers.Gold,
                >= 2000 => LoyaltyTiers.Silver,
                _ => LoyaltyTiers.Bronze,
            };
            customer.UpdatedAt = DateTime.UtcNow;

            var tx = new LoyaltyTransaction
            {
                CustomerId = customerId,
                OrderId = orderId,
                PointsChange = points,
                Reason = reason,
                Description = description,
                BalanceAfter = customer.LoyaltyPoints,
            };
            _ctx.LoyaltyTransactions.Add(tx);
            await _ctx.SaveChangesAsync();
            return new LoyaltyTxDto(tx.Id, tx.CustomerId, tx.PointsChange, tx.Reason, tx.Description, tx.BalanceAfter, tx.CreatedAt);
        }

        private static CustomerDto ToDto(Models.Customer c) => new(c.Id, c.RestaurantId, c.FullName, c.Phone, c.Email, c.LoyaltyPoints, c.LoyaltyTier, c.Note, c.CreatedAt);
    }
}
