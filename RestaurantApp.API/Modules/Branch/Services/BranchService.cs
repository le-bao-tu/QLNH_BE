using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Branch.DTOs;

namespace RestaurantApp.API.Modules.Branch.Services
{
    public interface IBranchService
    {
        Task<List<BranchDto>> GetByRestaurantAsync(Guid restaurantId);
        Task<BranchDto?> GetByIdAsync(Guid id);
        Task<BranchDto> CreateAsync(CreateBranchDto dto);
        Task<BranchDto?> UpdateAsync(Guid id, UpdateBranchDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

    public class BranchService : IBranchService
    {
        private readonly AppDbContext _context;

        public BranchService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BranchDto>> GetByRestaurantAsync(Guid restaurantId)
        {
            return await _context.Branches
                .Include(b => b.Tables)
                .Where(b => b.RestaurantId == restaurantId)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    RestaurantId = b.RestaurantId,
                    Name = b.Name,
                    Address = b.Address,
                    Phone = b.Phone,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    TableCount = b.Tables.Count(t => !t.IsDeleted)
                }).ToListAsync();
        }

        public async Task<BranchDto?> GetByIdAsync(Guid id)
        {
            return await _context.Branches
                .Include(b => b.Tables)
                .Where(b => b.Id == id)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    RestaurantId = b.RestaurantId,
                    Name = b.Name,
                    Address = b.Address,
                    Phone = b.Phone,
                    IsActive = b.IsActive,
                    CreatedAt = b.CreatedAt,
                    TableCount = b.Tables.Count(t => !t.IsDeleted)
                }).FirstOrDefaultAsync();
        }

        public async Task<BranchDto> CreateAsync(CreateBranchDto dto)
        {
            var branch = new Models.Branch
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone
            };
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(branch.Id) ?? throw new Exception("Không thể tạo chi nhánh");
        }

        public async Task<BranchDto?> UpdateAsync(Guid id, UpdateBranchDto dto)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return null;
            if (dto.Name != null) branch.Name = dto.Name;
            if (dto.Address != null) branch.Address = dto.Address;
            if (dto.Phone != null) branch.Phone = dto.Phone;
            if (dto.IsActive.HasValue) branch.IsActive = dto.IsActive.Value;
            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return false;
            branch.IsDeleted = true;
            branch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
