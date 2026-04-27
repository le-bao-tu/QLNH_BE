using Microsoft.AspNetCore.Mvc;
using RestaurantApp.API.Modules.Order.Services;

namespace RestaurantApp.API.Modules.Order.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overview/{workingId}")]
        public async Task<IActionResult> GetOverview(Guid workingId, [FromQuery] string mode = "branch", [FromQuery] string filter = "today")
        {
            return Ok(await _dashboardService.GetOverviewAsync(workingId, mode, filter));
        }

        [HttpGet("hourly/{workingId}")]
        public async Task<IActionResult> GetHourlyStats(Guid workingId, [FromQuery] string mode = "branch", [FromQuery] string filter = "today")
        {
            return Ok(await _dashboardService.GetTrendStatsAsync(workingId, mode, filter));
        }

        [HttpGet("category-revenue/{workingId}")]
        public async Task<IActionResult> GetCategoryRevenue(Guid workingId, [FromQuery] string mode = "branch", [FromQuery] string filter = "today")
        {
            return Ok(await _dashboardService.GetCategoryRevenueAsync(workingId, mode, filter));
        }
    }
}
