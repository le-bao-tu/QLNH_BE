using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Employee.Services;

namespace RestaurantApp.API.Modules.Employee.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _svc;
        public EmployeeController(IEmployeeService svc) => _svc = svc;

        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetByBranch(Guid branchId)
            => Ok(await _svc.GetByBranchAsync(branchId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _svc.GetByIdAsync(id); return r == null ? NotFound() : Ok(r);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var r = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = r.Id }, r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateEmployeeDto dto)
        {
            var r = await _svc.UpdateAsync(id, dto); return r == null ? NotFound() : Ok(r);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _svc.DeleteAsync(id); return ok ? NoContent() : NotFound();
        }

        [HttpGet("shifts/branch/{branchId}")]
        public async Task<IActionResult> GetShifts(Guid branchId, [FromQuery] DateOnly? date)
            => Ok(await _svc.GetShiftsAsync(branchId, date));

        [HttpPost("shifts")]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftDto dto)
            => Ok(await _svc.CreateShiftAsync(dto));

        [HttpPost("shifts/{shiftId}/checkin")]
        public async Task<IActionResult> CheckIn(Guid shiftId)
        {
            var r = await _svc.CheckInAsync(shiftId); return r == null ? NotFound() : Ok(r);
        }

        [HttpPost("shifts/{shiftId}/checkout")]
        public async Task<IActionResult> CheckOut(Guid shiftId)
        {
            var r = await _svc.CheckOutAsync(shiftId); return r == null ? NotFound() : Ok(r);
        }
    }
}
