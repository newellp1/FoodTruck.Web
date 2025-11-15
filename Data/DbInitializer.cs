using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace FoodTruck.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Customer", "Staff", "Admin" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin user
            var adminEmail = "admin@foodtruck.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}