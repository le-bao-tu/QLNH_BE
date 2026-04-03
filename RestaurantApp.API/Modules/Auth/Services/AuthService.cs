using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Auth.DTOs;
using RestaurantApp.API.Modules.Auth.Models;
using AutoMapper;

namespace RestaurantApp.API.Modules.Auth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly JwtProvider _jwtProvider;

        public AuthService(AppDbContext context, IMapper mapper, JwtProvider jwtProvider)
        {
            _context = context;
            _mapper = mapper;
            _jwtProvider = jwtProvider;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new Exception("Thông tin đăng nhập không chính xác");
            }

            // Với Owner: luôn tra cứu nhà hàng theo OwnerId để tránh sai do seed data
            if (user.Role == "Owner")
            {
                var realRestaurant = await _context.Restaurants
                    .FirstOrDefaultAsync(r => r.OwnerId == user.Id);

                if (realRestaurant != null && user.RestaurantId != realRestaurant.Id)
                {
                    var firstBranch = await _context.Branches
                        .FirstOrDefaultAsync(b => b.RestaurantId == realRestaurant.Id);
                    user.RestaurantId = realRestaurant.Id;
                    user.BranchId = firstBranch?.Id;
                    await _context.SaveChangesAsync();
                }
            }

            return _jwtProvider.GenerateToken(user);
        }

        public async Task<UserDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Set<User>().AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
            {
                throw new Exception("Tên đăng nhập hoặc email đã tồn tại");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Owner"
            };

            // Create Restaurant
            var restaurant = new RestaurantApp.API.Modules.Restaurant.Models.Restaurant
            {
                Id = Guid.NewGuid(),
                Name = string.IsNullOrEmpty(dto.RestaurantName) ? $"{dto.FullName}'s Restaurant" : dto.RestaurantName,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            // Create First Branch
            var branch = new RestaurantApp.API.Modules.Branch.Models.Branch
            {
                Id = Guid.NewGuid(),
                RestaurantId = restaurant.Id,
                Name = "Chi nhánh 1 (Main)",
                Address = "Chưa thiết lập",
                Phone = "Chưa thiết lập",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Link user to restaurant and branch
            user.RestaurantId = restaurant.Id;
            user.BranchId = branch.Id;

            _context.Set<User>().Add(user);
            _context.Restaurants.Add(restaurant);
            _context.Branches.Add(branch);

            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>Owner/Manager tạo tài khoản nhân viên gắn với nhà hàng/chi nhánh</summary>
        public async Task<UserDetailDto> CreateStaffAsync(CreateStaffDto dto)
        {
            if (await _context.Set<User>().AnyAsync(u => u.Username == dto.Username))
            {
                throw new Exception("Tên đăng nhập đã tồn tại");
            }

            if (!string.IsNullOrEmpty(dto.Email) &&
                await _context.Set<User>().AnyAsync(u => u.Email == dto.Email))
            {
                throw new Exception("Email đã được sử dụng");
            }

            // Validate restaurant & branch exist
            var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId && b.RestaurantId == dto.RestaurantId);
            if (!branchExists) throw new Exception("Chi nhánh không hợp lệ");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email ?? "",
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                RestaurantId = dto.RestaurantId,
                BranchId = dto.BranchId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            return MapToDetail(user);
        }

        /// <summary>Lấy tất cả tài khoản thuộc một nhà hàng</summary>
        public async Task<List<UserDetailDto>> GetUsersByRestaurantAsync(Guid restaurantId)
        {
            var users = await _context.Set<User>()
                .Where(u => u.RestaurantId == restaurantId)
                .OrderBy(u => u.Role).ThenBy(u => u.FullName)
                .ToListAsync();

            return users.Select(MapToDetail).ToList();
        }

        /// <summary>Xóa tài khoản nhân viên</summary>
        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _context.Set<User>().FindAsync(userId)
                ?? throw new Exception("Không tìm thấy tài khoản");

            if (user.Role == "Owner")
                throw new Exception("Không thể xoá tài khoản Owner");

            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync();
        }

        private static UserDetailDto MapToDetail(User u) => new UserDetailDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            Role = u.Role,
            RestaurantId = u.RestaurantId,
            BranchId = u.BranchId,
            CreatedAt = u.CreatedAt
        };
    }
}
