using RestaurantApp.API.Modules.Auth.DTOs;

namespace RestaurantApp.API.Modules.Auth.Services
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        Task<UserDetailDto> CreateStaffAsync(CreateStaffDto dto);
        Task<List<UserDetailDto>> GetUsersByRestaurantAsync(Guid restaurantId, Guid? branchId = null);
        Task<UserDetailDto?> UpdateUserAsync(Guid userId, UpdateStaffDto dto);
        Task DeleteUserAsync(Guid userId);
    }
}
