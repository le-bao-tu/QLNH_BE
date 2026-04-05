using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Promotion.Models;

namespace RestaurantApp.API.Modules.Promotion.Services
{
    public record PromotionDto(Guid Id, Guid RestaurantId, string Name, string? Description, string Type, string ApplyTo, decimal DiscountValue, decimal MinOrderAmount, decimal? MaxDiscount, DateOnly StartDate, DateOnly EndDate, bool IsActive, string? MenuItemIds, string? BranchIds);
    public record VoucherDto(Guid Id, Guid PromotionId, string Code, int UsageLimit, int UsedCount, Guid? AssignedTo, DateTime? ExpiresAt, bool IsValid);
    public record CreatePromotionDto(Guid RestaurantId, string Name, string? Description, string Type, string ApplyTo, decimal DiscountValue, decimal MinOrderAmount, decimal? MaxDiscount, DateOnly StartDate, DateOnly EndDate, string? MenuItemIds, string? BranchIds);
    public record UpdatePromotionDto(string? Name, string? Description, string? Type, string? ApplyTo, decimal? DiscountValue, decimal? MinOrderAmount, decimal? MaxDiscount, DateOnly? StartDate, DateOnly? EndDate, bool? IsActive, string? MenuItemIds, string? BranchIds);
    public record CreateVoucherDto(Guid PromotionId, string Code, int UsageLimit, Guid? AssignedTo, DateTime? ExpiresAt);

    public record ValidateVoucherResultDto(Guid VoucherId, string Code, PromotionDto Promotion);

    public interface IPromotionService
    {
        Task<List<PromotionDto>> GetByRestaurantAsync(Guid restaurantId, bool? activeOnly = null);
        Task<List<PromotionDto>> GetActivePromotionsAsync(Guid restaurantId);
        Task<ValidateVoucherResultDto?> ValidateVoucherAsync(string code, decimal orderAmount);
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto);
        Task<List<VoucherDto>> GetVouchersByPromotionAsync(Guid promotionId);
        Task<decimal> ApplyPromotionAsync(Guid promotionId, decimal orderTotal);
        Task<PromotionDto> CreateAsync(CreatePromotionDto dto);
        Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto);
        Task<PromotionDto?> ToggleActiveAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<List<PromotionDto>> GetActiveItemPromotionsAsync(Guid restaurantId);

    }

    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _ctx;
        public PromotionService(AppDbContext ctx) => _ctx = ctx;

        public async Task<List<PromotionDto>> GetByRestaurantAsync(Guid restaurantId, bool? activeOnly = null)
        {
            var query = _ctx.Promotions.Where(p => p.RestaurantId == restaurantId && !p.IsDeleted);
            if (activeOnly == true) query = query.Where(p => p.IsActive);
            return await query.OrderByDescending(p => p.CreatedAt).Select(p => ToDto(p)).ToListAsync();
        }

        public async Task<List<PromotionDto>> GetActivePromotionsAsync(Guid restaurantId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var promotions = await _ctx.Promotions
                .Where(p => p.RestaurantId == restaurantId && p.IsActive && p.StartDate <= today && p.EndDate >= today)
                .ToListAsync();
            return promotions.Select(ToDto).ToList();
        }

        public async Task<List<PromotionDto>> GetActiveItemPromotionsAsync(Guid restaurantId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _ctx.Promotions
                .Where(p => p.RestaurantId == restaurantId 
                    && p.IsActive 
                    && !p.IsDeleted
                    && p.ApplyTo == "item"
                    && p.StartDate <= today 
                    && p.EndDate >= today)
                .Select(p => ToDto(p))
                .ToListAsync();
        }

        public async Task<PromotionDto> CreateAsync(CreatePromotionDto dto)
        {
            var p = new Models.Promotion
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                ApplyTo = dto.ApplyTo,
                DiscountValue = dto.DiscountValue,
                MinOrderAmount = dto.MinOrderAmount,
                MaxDiscount = dto.MaxDiscount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MenuItemIds = dto.MenuItemIds,
                BranchIds = dto.BranchIds
            };
            _ctx.Promotions.Add(p);
            await _ctx.SaveChangesAsync();
            return ToDto(p);
        }

        public async Task<PromotionDto?> UpdateAsync(Guid id, UpdatePromotionDto dto)
        {
            var p = await _ctx.Promotions.FindAsync(id);
            if (p == null) return null;
            if (dto.Name != null) p.Name = dto.Name;
            if (dto.Description != null) p.Description = dto.Description;
            if (dto.Type != null) p.Type = dto.Type;
            if (dto.ApplyTo != null) p.ApplyTo = dto.ApplyTo;
            if (dto.DiscountValue.HasValue) p.DiscountValue = dto.DiscountValue.Value;
            if (dto.MinOrderAmount.HasValue) p.MinOrderAmount = dto.MinOrderAmount.Value;
            if (dto.MaxDiscount.HasValue) p.MaxDiscount = dto.MaxDiscount.Value;
            if (dto.StartDate.HasValue) p.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) p.EndDate = dto.EndDate.Value;
            if (dto.IsActive.HasValue) p.IsActive = dto.IsActive.Value;
            p.MenuItemIds = dto.MenuItemIds;
            p.BranchIds = dto.BranchIds;
            p.UpdatedAt = DateTime.UtcNow;
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

        public async Task<bool> DeleteAsync(Guid id)
        {
            var p = await _ctx.Promotions.FindAsync(id);
            if (p == null) return false;
            p.IsDeleted = true;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<ValidateVoucherResultDto?> ValidateVoucherAsync(string code, decimal orderAmount)
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

            if (!isValid) return null;

            return new ValidateVoucherResultDto(voucher.Id, voucher.Code, ToDto(voucher.Promotion!));
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto dto)
        {
            var v = new VoucherCode { PromotionId = dto.PromotionId, Code = dto.Code.ToUpper(), UsageLimit = dto.UsageLimit, AssignedTo = dto.AssignedTo, ExpiresAt = dto.ExpiresAt };
            _ctx.VoucherCodes.Add(v);
            await _ctx.SaveChangesAsync();
            return new VoucherDto(v.Id, v.PromotionId, v.Code, v.UsageLimit, v.UsedCount, v.AssignedTo, v.ExpiresAt, true);
        }

        public async Task<List<VoucherDto>> GetVouchersByPromotionAsync(Guid promotionId)
        {
            return await _ctx.VoucherCodes
                .Where(v => v.PromotionId == promotionId && !v.IsDeleted)
                .Select(v => new VoucherDto(v.Id, v.PromotionId, v.Code, v.UsageLimit, v.UsedCount, v.AssignedTo, v.ExpiresAt, true))
                .ToListAsync();
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

        private static PromotionDto ToDto(Models.Promotion p) => new(p.Id, p.RestaurantId, p.Name, p.Description, p.Type, p.ApplyTo, p.DiscountValue, p.MinOrderAmount, p.MaxDiscount, p.StartDate, p.EndDate, p.IsActive, p.MenuItemIds, p.BranchIds);
    }
}
