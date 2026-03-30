using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Promotion.Models;

namespace RestaurantApp.API.Modules.Promotion.Services
{
    public record PromotionDto(Guid Id, Guid RestaurantId, string Name, string? Description, string Type, decimal DiscountValue, decimal MinOrderAmount, decimal? MaxDiscount, DateOnly StartDate, DateOnly EndDate, bool IsActive);
    public record VoucherDto(Guid Id, Guid PromotionId, string Code, int UsageLimit, int UsedCount, Guid? AssignedTo, DateTime? ExpiresAt, bool IsValid);
    public record CreatePromotionDto(Guid RestaurantId, string Name, string? Description, string Type, decimal DiscountValue, decimal MinOrderAmount, decimal? MaxDiscount, DateOnly StartDate, DateOnly EndDate);
    public record CreateVoucherDto(Guid PromotionId, string Code, int UsageLimit, Guid? AssignedTo, DateTime? ExpiresAt);

    public interface IPromotionService
    {
        Task<List<PromotionDto>> GetByRestaurantAsync(Guid restaurantId, bool? activeOnly = null);
        Task<PromotionDto> CreateAsync(CreatePromotionDto dto);
        Task<PromotionDto?> ToggleActiveAsync(Guid id);
        Task<VoucherDto?> ValidateVoucherAsync(string code, decimal orderAmount);
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto);
        Task<decimal> ApplyPromotionAsync(Guid promotionId, decimal orderTotal);
    }

    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _ctx;
        public PromotionService(AppDbContext ctx) => _ctx = ctx;

        public async Task<List<PromotionDto>> GetByRestaurantAsync(Guid restaurantId, bool? activeOnly = null)
        {
            var query = _ctx.Promotions.Where(p => p.RestaurantId == restaurantId);
            if (activeOnly == true) query = query.Where(p => p.IsActive);
            return await query.OrderByDescending(p => p.CreatedAt).Select(p => ToDto(p)).ToListAsync();
        }

        public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto)
        {
            var p = new Models.Promotion
            {
                RestaurantId = dto.RestaurantId, Name = dto.Name, Description = dto.Description,
                Type = dto.Type, DiscountValue = dto.DiscountValue, MinOrderAmount = dto.MinOrderAmount,
                MaxDiscount = dto.MaxDiscount, StartDate = dto.StartDate, EndDate = dto.EndDate,
            };
            _ctx.Promotions.Add(p);
            await _ctx.SaveChangesAsync();
            return ToDto(p);
        }

        public async Task<PromotionDto?> ToggleActiveAsync(Guid id)
        {
            var p = await _ctx.Promotions.FindAsync(id);
            if (p == null) return null;
            p.IsActive = !p.IsActive;
            await _ctx.SaveChangesAsync();
            return ToDto(p);
        }

        public async Task<VoucherDto?> ValidateVoucherAsync(string code, decimal orderAmount)
        {
            var voucher = await _ctx.VoucherCodes
                .Include(v => v.Promotion)
                .FirstOrDefaultAsync(v => v.Code == code && !v.IsDeleted);

            if (voucher == null) return null;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var isValid = voucher.Promotion != null
                && voucher.Promotion.IsActive
                && voucher.Promotion.StartDate <= today
                && voucher.Promotion.EndDate >= today
                && (voucher.UsageLimit == 0 || voucher.UsedCount < voucher.UsageLimit)
                && (voucher.ExpiresAt == null || voucher.ExpiresAt > DateTime.UtcNow)
                && orderAmount >= voucher.Promotion.MinOrderAmount;

            return new VoucherDto(voucher.Id, voucher.PromotionId, voucher.Code, voucher.UsageLimit, voucher.UsedCount, voucher.AssignedTo, voucher.ExpiresAt, isValid);
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto)
        {
            var v = new VoucherCode { PromotionId = dto.PromotionId, Code = dto.Code.ToUpper(), UsageLimit = dto.UsageLimit, AssignedTo = dto.AssignedTo, ExpiresAt = dto.ExpiresAt };
            _ctx.VoucherCodes.Add(v);
            await _ctx.SaveChangesAsync();
            return new VoucherDto(v.Id, v.PromotionId, v.Code, v.UsageLimit, v.UsedCount, v.AssignedTo, v.ExpiresAt, true);
        }

        public async Task<decimal> ApplyPromotionAsync(Guid promotionId, decimal orderTotal)
        {
            var p = await _ctx.Promotions.FindAsync(promotionId);
            if (p == null) return 0;

            return p.Type switch
            {
                PromotionType.PercentDiscount => Math.Min(orderTotal * p.DiscountValue / 100, p.MaxDiscount ?? decimal.MaxValue),
                PromotionType.FixedDiscount => Math.Min(p.DiscountValue, orderTotal),
                _ => 0,
            };
        }

        private static PromotionDto ToDto(Models.Promotion p) => new(p.Id, p.RestaurantId, p.Name, p.Description, p.Type, p.DiscountValue, p.MinOrderAmount, p.MaxDiscount, p.StartDate, p.EndDate, p.IsActive);
    }
}
