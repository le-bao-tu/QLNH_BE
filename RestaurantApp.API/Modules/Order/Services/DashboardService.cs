using Microsoft.EntityFrameworkCore;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Order.DTOs;
using RestaurantApp.API.Modules.Order.Models;
using RestaurantApp.API.Modules.Table.Models;
using RestaurantApp.API.Modules.Inventory.Models;
using RestaurantApp.API.Modules.Menu.Models;

namespace RestaurantApp.API.Modules.Order.Services
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDto> GetOverviewAsync(Guid workingId, string mode, string filter);
        Task<List<TrendStatsDto>> GetTrendStatsAsync(Guid workingId, string mode, string filter);
        Task<List<CategoryRevenueDto>> GetCategoryRevenueAsync(Guid workingId, string mode, string filter);
    }

    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Models.Order> GetFilteredOrders(Guid workingId, string mode, string filter)
        {
            var query = _context.Orders.AsQueryable();

            if (mode == "branch")
            {
                query = query.Where(o => o.BranchId == workingId);
            }
            else
            {
                query = query.Where(o => o.RestaurantId == workingId);
            }

            var now = DateTime.UtcNow;
            var startDate = now.Date;

            if (filter == "week")
            {
                int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                startDate = now.AddDays(-1 * diff).Date;
            }
            else if (filter == "month")
            {
                startDate = new DateTime(now.Year, now.Month, 1);
            }
            // "today" is default (now.Date)

            return query.Where(o => o.CreatedAt >= startDate && o.Status == OrderStatus.Paid);
        }

        public async Task<DashboardOverviewDto> GetOverviewAsync(Guid workingId, string mode, string filter)
        {
            var ordersQuery = GetFilteredOrders(workingId, mode, filter);
            
            var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount);
            var orderCount = await ordersQuery.CountAsync();
            var avgOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

            var tablesQuery = _context.Tables.AsQueryable();
            if (mode == "branch") tablesQuery = tablesQuery.Where(t => t.BranchId == workingId);
            else tablesQuery = tablesQuery.Where(t => t.RestaurantId == workingId);

            var totalTables = await tablesQuery.CountAsync();
            var occupiedTables = await tablesQuery.CountAsync(t => t.Status == TableStatus.Occupied);

            return new DashboardOverviewDto
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AverageOrderValue = avgOrderValue,
                OccupiedTables = occupiedTables,
                TotalTables = totalTables,
            };
        }

        public async Task<List<TrendStatsDto>> GetTrendStatsAsync(Guid workingId, string mode, string filter)
        {
            var orders = await GetFilteredOrders(workingId, mode, filter)
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync();

            var result = new List<TrendStatsDto>();
            var now = DateTime.UtcNow.AddHours(7); // Local time

            if (filter == "week")
            {
                // Group by Day of Week
                var days = new[] { "Hai", "Ba", "Tư", "Năm", "Sáu", "Bảy", "CN" };
                var grouped = orders
                    .GroupBy(o => (int)o.CreatedAt.AddHours(7).DayOfWeek)
                    .ToDictionary(g => g.Key, g => new { Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() });

                for (int i = 1; i <= 7; i++)
                {
                    int dw = i == 7 ? 0 : i; // DayOfWeek: Sunday is 0
                    result.Add(new TrendStatsDto
                    {
                        Label = days[i - 1],
                        Revenue = grouped.ContainsKey(dw) ? grouped[dw].Revenue : 0,
                        OrderCount = grouped.ContainsKey(dw) ? grouped[dw].Count : 0,
                        SortOrder = i
                    });
                }
            }
            else if (filter == "month")
            {
                // Group by Day of Month
                var grouped = orders
                    .GroupBy(o => o.CreatedAt.AddHours(7).Day)
                    .ToDictionary(g => g.Key, g => new { Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() });

                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                for (int i = 1; i <= daysInMonth; i++)
                {
                    result.Add(new TrendStatsDto
                    {
                        Label = i.ToString("D2"),
                        Revenue = grouped.ContainsKey(i) ? grouped[i].Revenue : 0,
                        OrderCount = grouped.ContainsKey(i) ? grouped[i].Count : 0,
                        SortOrder = i
                    });
                }
            }
            else
            {
                // Group by Hour (Today)
                var grouped = orders
                    .GroupBy(o => o.CreatedAt.AddHours(7).Hour)
                    .ToDictionary(g => g.Key, g => new { Revenue = g.Sum(x => x.TotalAmount), Count = g.Count() });

                for (int i = 0; i < 24; i++)
                {
                    result.Add(new TrendStatsDto
                    {
                        Label = i.ToString("00") + "h",
                        Revenue = grouped.ContainsKey(i) ? grouped[i].Revenue : 0,
                        OrderCount = grouped.ContainsKey(i) ? grouped[i].Count : 0,
                        SortOrder = i
                    });
                }
            }

            return result.OrderBy(s => s.SortOrder).ToList();
        }

        public async Task<List<CategoryRevenueDto>> GetCategoryRevenueAsync(Guid workingId, string mode, string filter)
        {
            var ordersQuery = GetFilteredOrders(workingId, mode, filter);

            var categoryData = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.MenuItem)
                    .ThenInclude(m => m!.Category)
                .Where(oi => oi.Order!.Status == OrderStatus.Paid)
                .Where(oi => mode == "branch" ? oi.Order!.BranchId == workingId : oi.Order!.RestaurantId == workingId)
                // Re-apply date filter for order items as as well if needed, but easier to join
                .ToListAsync(); // Pull some data to memory for grouping if complex, but EF can handle some.

            // Since we already have a helper for filtered orders, let's use it
            var paidOrderIds = await ordersQuery.Select(o => o.Id).ToListAsync();

            var stats = await _context.OrderItems
                .Include(oi => oi.MenuItem)
                    .ThenInclude(m => m!.Category)
                .Where(oi => paidOrderIds.Contains(oi.OrderId))
                .GroupBy(oi => oi.MenuItem!.Category!.Name)
                .Select(g => new CategoryRevenueDto
                {
                    CategoryName = g.Key ?? "Chưa phân loại",
                    Revenue = g.Sum(oi => oi.TotalPrice),
                    Quantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(s => s.Revenue)
                .ToListAsync();

            return stats;
        }
    }
}
