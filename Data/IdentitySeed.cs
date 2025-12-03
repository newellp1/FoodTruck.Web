using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTruck.Web.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

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