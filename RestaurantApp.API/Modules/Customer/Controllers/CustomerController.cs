using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Customer.Services;
using RestaurantApp.API.Common;

namespace RestaurantApp.API.Modules.Customer.Controllers
{
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _svc;
        public CustomerController(ICustomerService svc) => _svc = svc;

        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId, [FromQuery] PaginationParams @params)
        {
            if (@params.PageIndex > 0)
                return Ok(await _svc.GetByRestaurantPagedAsync(restaurantId, @params));
            return Ok(await _svc.GetByRestaurantAsync(restaurantId, @params.Search));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpGet("by-phone/{restaurantId}/{phone}")]
        public async Task<IActionResult> GetByPhone(Guid restaurantId, string phone)
        {
            var r = await _svc.GetByPhoneAsync(restaurantId, phone);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var r = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = r.Id }, r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCustomerDto dto)
        {
            var r = await _svc.UpdateAsync(id, dto);
            return r == null ? NotFound() : Ok(r);
        }

        [HttpGet("{id}/loyalty")]
        public async Task<IActionResult> GetLoyaltyHistory(Guid id)
            => Ok(await _svc.GetLoyaltyHistoryAsync(id));

        [HttpPost("{id}/loyalty")]
        public async Task<IActionResult> AddPoints(Guid id, [FromBody] AddPointsDto dto)
        {
            var r = await _svc.AddLoyaltyPointsAsync(id, dto.Points, dto.Reason, dto.Description, dto.OrderId);
            return Ok(r);
        }
    }

    public record AddPointsDto(int Points, string Reason, string? Description, Guid? OrderId);
}
