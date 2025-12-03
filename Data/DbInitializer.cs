using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodTruck.Web.Data
{
    /// Database / Identity seeding helper.
    /// 
    /// Responsibilities:
    ///  - Apply any pending EF Core migrations.
    ///  - Ensure required roles exist (Customer, Staff, Admin).
    ///  - Ensure a default Admin user exists and is in the Admin role.
    /// 
    /// Call this once at startup from Program.cs:
    ///   await DbInitializer.SeedAsync(app);
    public static class DbInitializer
    {
        /// Entry point for seeding the database and identity data.
        /// <param name="app">The configured WebApplication.</param>
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            // Create a scope so we can resolve scoped services
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            // Resolve DbContext, RoleManager, and UserManager from DI
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // ----------------------------------------------------
            // 1) Ensure database is created with current schema
            //    This creates the database and all tables if they don't exist.
            // ----------------------------------------------------
            await context.Database.EnsureCreatedAsync();

            // ----------------------------------------------------
            // 2) Ensure core roles exist
            //    - Customer: default role for regular users
            //    - Staff: kitchen / order queue operators
            //    - Admin: full back-office access
            // ----------------------------------------------------
            string[] roles = { "Customer", "Staff", "Admin" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // ----------------------------------------------------
            // 3) Ensure a default Admin user exists
            //    Change the email/password here if needed.
            // ----------------------------------------------------

            // Use YOUR actual project admin email here
            const string adminEmail = "newellp1@etsu.edu";
            const string adminPassword = "Admin123!"; // For demo/dev only – change for real deployment

            // Try to find the user by email
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // If not found, create a new ApplicationUser
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true // no email confirmation needed for class demo
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    // If something goes wrong, you can log errors here.
                    // For the class project we just throw so you see it immediately.
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to create admin user: {errors}");
                }
            }

            // Ensure the admin user is in the Admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // (Optional) Also make the admin a Customer, if you want them to place orders too
            if (!await userManager.IsInRoleAsync(adminUser, "Customer"))
            {
                await userManager.AddToRoleAsync(adminUser, "Customer");
            }

            // ----------------------------------------------------
            // 4) Optional: seed demo domain data (trucks, locations,
            //    menu categories, items, etc.).
            //    You can add this later if you want.
            // ----------------------------------------------------
            // Example (pseudo-code):
            // if (!context.Trucks.Any()) { ... seed a demo truck ... }
        }
    }
}