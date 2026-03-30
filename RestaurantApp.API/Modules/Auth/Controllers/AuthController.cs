using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Auth.DTOs;
using RestaurantApp.API.Modules.Auth.Services;

namespace RestaurantApp.API.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = await _authService.RegisterAsync(dto);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _authService.LoginAsync(dto);
                
                // Set cookie as per plan
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Should be true in production
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1)
                };
                
                Response.Cookies.Append("accessToken", token, cookieOptions);
                
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            return Ok(new { message = "Logged out" });
        }
    }
}
