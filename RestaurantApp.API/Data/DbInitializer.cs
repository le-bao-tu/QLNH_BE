using RestaurantApp.API.Modules.Restaurant.Models;
using RestaurantApp.API.Modules.Branch.Models;
using RestaurantApp.API.Modules.Table.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantApp.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var restaurantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var branchId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // 1. Seed Restaurant
            if (!await context.Restaurants.AnyAsync(r => r.Id == restaurantId))
            {
                context.Restaurants.Add(new Restaurant
                {
                    Id = restaurantId,
                    Name = "Pfxsoft Restaurant Group",
                    OwnerId = Guid.NewGuid(),
                    TaxCode = "123456789"
                });
            }

            // 2. Seed Branch
            if (!await context.Branches.AnyAsync(b => b.Id == branchId))
            {
                context.Branches.Add(new Branch
                {
                    Id = branchId,
                    RestaurantId = restaurantId,
                    Name = "Chi nhánh Trung tâm (HQ)",
                    Address = "123 Đường Pfxsoft, TP. Hồ Chí Minh",
                    Phone = "0901234567",
                    IsActive = true
                });
            }

            // 3. Seed some default Tables if none exist
            if (!await context.Tables.AnyAsync(t => t.BranchId == branchId))
            {
                for (int i = 1; i <= 12; i++)
                {
                    context.Tables.Add(new Table
                    {
                        Id = Guid.NewGuid(),
                        BranchId = branchId,
                        TableNumber = i,
                        Capacity = 4,
                        Status = "available",
                        Floor = 1
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
