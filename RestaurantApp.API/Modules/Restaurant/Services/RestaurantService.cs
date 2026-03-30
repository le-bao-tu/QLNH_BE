using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Restaurant.DTOs;

namespace RestaurantApp.API.Modules.Restaurant.Services
{
    public interface IRestaurantService
    {
        Task<List<RestaurantDto>> GetAllAsync();
        Task<RestaurantDto?> GetByIdAsync(Guid id);
        Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, Guid ownerId);
        Task<RestaurantDto?> UpdateAsync(Guid id, UpdateRestaurantDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class RestaurantService : IRestaurantService
    {
        private readonly AppDbContext _context;

        public RestaurantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RestaurantDto>> GetAllAsync()
        {
            return await _context.Restaurants
                .Include(r => r.Branches)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Address = r.Address,
                    Phone = r.Phone,
                    Email = r.Email,
                    LogoUrl = r.LogoUrl,
                    OwnerId = r.OwnerId,
                    CreatedAt = r.CreatedAt,
                    BranchCount = r.Branches.Count(b => !b.IsDeleted)
                }).ToListAsync();
        }

        public async Task<RestaurantDto?> GetByIdAsync(Guid id)
        {
            return await _context.Restaurants
                .Include(r => r.Branches)
                .Where(r => r.Id == id)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Address = r.Address,
                    Phone = r.Phone,
                    Email = r.Email,
                    LogoUrl = r.LogoUrl,
                    OwnerId = r.OwnerId,
                    CreatedAt = r.CreatedAt,
                    BranchCount = r.Branches.Count(b => !b.IsDeleted)
                }).FirstOrDefaultAsync();
        }

        public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, Guid ownerId)
        {
            var restaurant = new Models.Restaurant
            {
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                LogoUrl = dto.LogoUrl,
                OwnerId = ownerId
            };
            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(restaurant.Id) ?? throw new Exception("Không thể tạo nhà hàng");
        }

        public async Task<RestaurantDto?> UpdateAsync(Guid id, UpdateRestaurantDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return null;
            if (dto.Name != null) restaurant.Name = dto.Name;
            if (dto.Description != null) restaurant.Description = dto.Description;
            if (dto.Address != null) restaurant.Address = dto.Address;
            if (dto.Phone != null) restaurant.Phone = dto.Phone;
            if (dto.Email != null) restaurant.Email = dto.Email;
            if (dto.LogoUrl != null) restaurant.LogoUrl = dto.LogoUrl;
            restaurant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return false;
            restaurant.IsDeleted = true;
            restaurant.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
