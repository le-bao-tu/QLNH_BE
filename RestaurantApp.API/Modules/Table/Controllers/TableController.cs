using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Table.DTOs;
using RestaurantApp.API.Modules.Table.Services;

namespace RestaurantApp.API.Modules.Table.Controllers
{
    [ApiController]
    [Route("api/tables")]
    [Authorize]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }

        [HttpGet("branch/{branchId?}")]
        public async Task<IActionResult> GetByBranch(Guid branchId)
        {
            var tables = await _tableService.GetByBranchAsync(branchId);
            return Ok(tables);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var table = await _tableService.GetByIdAsync(id);
            return table == null ? NotFound() : Ok(table);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTableDto dto)
        {
            try
            {
                var table = await _tableService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
            }
            catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTableDto dto)
        {
            var table = await _tableService.UpdateAsync(id, dto);
            return table == null ? NotFound() : Ok(table);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTableStatusRequest req)
        {
            var table = await _tableService.UpdateStatusAsync(id, req.Status);
            return table == null ? NotFound() : Ok(table);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _tableService.DeleteAsync(id);
            return success ? Ok(new { message = "Đã xóa bàn" }) : NotFound();
        }
    }

    public class UpdateTableStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
