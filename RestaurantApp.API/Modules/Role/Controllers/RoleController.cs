using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Role.DTOs;
using RestaurantApp.API.Modules.Role.Services;

namespace RestaurantApp.API.Modules.Role.Controllers
{
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetRoles(Guid restaurantId)
        {
            var roles = await _roleService.GetRolesAsync(restaurantId);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var role = await _roleService.CreateRoleAsync(dto);
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
        {
            var role = await _roleService.UpdateRoleAsync(id, dto);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
