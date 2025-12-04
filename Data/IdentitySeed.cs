using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTruck.Web.Data
{
    /// Seed initial roles and admin user for the application.
    public static class IdentitySeed
    {
        /// Ensure the Admin role exists and assign it to the default admin user.
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure database is created first
            await context.Database.EnsureCreatedAsync();

            const string adminRoleName = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRoleName))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRoleName));
            }

            const string adminEmail = "newellp1@etsu.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }
        }
    }
}