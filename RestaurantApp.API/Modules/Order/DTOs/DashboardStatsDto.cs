using System;
using System.Collections.Generic;

namespace RestaurantApp.API.Modules.Order.DTOs
{
    public class DashboardOverviewDto
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int OccupiedTables { get; set; }
        public int TotalTables { get; set; }
        public int LowStockCount { get; set; }
    }

    public class TrendStatsDto
    {
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int SortOrder { get; set; } // Optional: to help with sorting if needed
    }

    public class CategoryRevenueDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Quantity { get; set; }
    }
}
