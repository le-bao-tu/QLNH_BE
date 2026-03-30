using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Promotion.Services;

namespace RestaurantApp.API.Modules.Promotion.Controllers
{
    [ApiController]
    [Route("api/promotions")]
    [Authorize]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _svc;
        public PromotionController(IPromotionService svc) => _svc = svc;

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId, [FromQuery] bool? activeOnly)
            => Ok(await _svc.GetByRestaurantAsync(restaurantId, activeOnly));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePromotionDto dto)
            => Ok(await _svc.CreateAsync(dto));

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> Toggle(Guid id)
        {
            var r = await _svc.ToggleActiveAsync(id); return r == null ? NotFound() : Ok(r);
        }

        [HttpPost("vouchers")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherDto dto)
            => Ok(await _svc.CreateVoucherAsync(dto));

        [HttpGet("vouchers/validate")]
        public async Task<IActionResult> ValidateVoucher([FromQuery] string code, [FromQuery] decimal orderAmount)
        {
            var r = await _svc.ValidateVoucherAsync(code, orderAmount);
            return r == null ? NotFound("Mã voucher không tồn tại") : Ok(r);
        }
    }
}
