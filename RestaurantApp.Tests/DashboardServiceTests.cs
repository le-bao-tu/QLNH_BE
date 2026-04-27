using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RestaurantApp.API.Data;
using RestaurantApp.API.Modules.Order.Models;
using RestaurantApp.API.Modules.Order.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantApp.Tests
{
    public class DashboardServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) });
            context.User = new ClaimsPrincipal(identity);
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            return new AppDbContext(options, mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task GetOverviewAsync_ShouldReturnCorrectData()
        {
            // Arrange
            var context = GetDbContext();
            var service = new DashboardService(context);
            var branchId = Guid.NewGuid();

            context.Orders.Add(new Order 
            { 
                Id = Guid.NewGuid(), 
                BranchId = branchId, 
                TotalAmount = 100000, 
                Status = OrderStatus.Paid, 
                CreatedAt = DateTime.UtcNow 
            });
            context.Orders.Add(new Order 
            { 
                Id = Guid.NewGuid(), 
                BranchId = branchId, 
                TotalAmount = 200000, 
                Status = OrderStatus.Paid, 
                CreatedAt = DateTime.UtcNow 
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetOverviewAsync(branchId, "branch", "today");

            // Assert
            Assert.Equal(300000, result.TotalRevenue);
            Assert.Equal(2, result.OrderCount);
            Assert.Equal(150000, result.AverageOrderValue);
        }

        [Fact]
        public async Task GetTrendStatsAsync_ShouldReturn24HoursForToday()
        {
            // Arrange
            var context = GetDbContext();
            var service = new DashboardService(context);
            var branchId = Guid.NewGuid();

            // Act
            var result = await service.GetTrendStatsAsync(branchId, "branch", "today");

            // Assert
            Assert.Equal(24, result.Count);
            Assert.Equal("00h", result[0].Label);
            Assert.Equal("23h", result[23].Label);
        }
    }
}
