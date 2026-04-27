using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Role.DTOs;
using RestaurantApp.API.Modules.Role.Models;
using System.Text.Json;

namespace RestaurantApp.API.Modules.Role.Services
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetRolesAsync(Guid restaurantId);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
        Task<bool> DeleteRoleAsync(Guid id);
    }

    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;

        public RoleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoleDto>> GetRolesAsync(Guid restaurantId)
        {
            var roles = await _context.RestaurantRoles
                .Where(r => r.RestaurantId == restaurantId)
                .ToListAsync();

            return roles.Select(r => new RoleDto
            {
                Id = r.Id,
                RestaurantId = (Guid)r.RestaurantId,
                Name = r.Name,
                Description = r.Description,
                Permissions = JsonSerializer.Deserialize<List<string>>(r.Permissions) ?? new List<string>()
            }).ToList();
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new RestaurantRole
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Permissions = JsonSerializer.Serialize(dto.Permissions)
            };

            _context.RestaurantRoles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                RestaurantId = (Guid)role.RestaurantId,
                Name = role.Name,
                Description = role.Description,
                Permissions = dto.Permissions
            };
        }

        public async Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto dto)
        {
            var role = await _context.RestaurantRoles.FindAsync(id);
            if (role == null) return null;

            role.Name = dto.Name;
            role.Description = dto.Description;
            role.Permissions = JsonSerializer.Serialize(dto.Permissions);
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new RoleDto
            {
                Id = role.Id,
                RestaurantId = (Guid)role.RestaurantId,
                Name = role.Name,
                Description = role.Description,
                Permissions = dto.Permissions
            };
        }

        public async Task<bool> DeleteRoleAsync(Guid id)
        {
            var role = await _context.RestaurantRoles.FindAsync(id);
            if (role == null) return false;

            role.IsDeleted = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
