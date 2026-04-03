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
                Role = "Owner" // Highest role
            };

            _context.Set<User>().Add(user);

            // Create Restaurant (Step 1.2)
            var restaurant = new RestaurantApp.API.Modules.Restaurant.Models.Restaurant
            {
                Id = Guid.NewGuid(),
                Name = string.IsNullOrEmpty(dto.RestaurantName) ? $"{dto.FullName}'s Restaurant" : dto.RestaurantName,
                OwnerId = user.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Restaurants.Add(restaurant);

            // Create First Branch (Step 1.3)
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
            _context.Branches.Add(branch);

            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }
    }
}
