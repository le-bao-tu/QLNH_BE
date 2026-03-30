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
                Username = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Manager" // Default for registered users in this context?
            };

            _context.Set<User>().Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }
    }
}
