using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrderSystem.Infrastructure.Identity;
using OrderSystem.Infrastructure.Persistence;

namespace OrderSystem.Infrastructure.Persistence
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var admin = new ApplicationUser { UserName = "admin", Role = "Admin", CreatedBy = "System" };
                await userManager.CreateAsync(admin, "Admin@123");

                var manager = new ApplicationUser { UserName = "manager", Role = "Manager", CreatedBy = "System" };
                await userManager.CreateAsync(manager, "Manager@123");

                var user = new ApplicationUser { UserName = "user", Role = "User", CreatedBy = "System" };
                await userManager.CreateAsync(user, "User@123");
            }

            if (!context.Orders.Any())
            {
                context.Orders.AddRange(
                    new OrderSystem.Domain.Entities.Order { Description = "Corporate Laptop Bundle", Amount = 12500, CreatedBy = "admin", Status = OrderSystem.Domain.Enums.OrderStatus.Pending },
                    new OrderSystem.Domain.Entities.Order { Description = "Cloud Infrastructure Renewal", Amount = 45000, CreatedBy = "manager", Status = OrderSystem.Domain.Enums.OrderStatus.Approved },
                    new OrderSystem.Domain.Entities.Order { Description = "Office Supplies Q3", Amount = 850, CreatedBy = "user", Status = OrderSystem.Domain.Enums.OrderStatus.Rejected }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
