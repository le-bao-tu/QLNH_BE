using RestaurantApp.API.Modules.Auth.DTOs;

namespace RestaurantApp.API.Modules.Auth.Services
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
    }
}
