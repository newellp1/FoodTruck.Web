using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Data
{
    /// Main EF Core DbContext for the application.
    /// Inherits from IdentityDbContext<ApplicationUser> so it includes:
    /// - ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, etc.)
    /// - All of the FoodTruck domain tables (Menu, Orders, Schedules, etc.)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        /// Constructor used by ASP.NET Core dependency injection.
        /// The options (connection string, provider, etc.) are passed in from Program.cs.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ============================
        // DbSet properties (tables)
        // Each DbSet<T> represents a table in SQL Server.
        // ============================

         /// Each Physical Food Truck
        public DbSet<Truck> Trucks => Set<Truck>();


        /// Locations the truck can park (address)
        public DbSet<Location> Locations => Set<Location>();


        /// Schedules that connect a Truck to a Location during a time window.
        /// Drives "where is the truck now" and whether ordering is enabled.
        public DbSet<Schedule> Schedules => Set<Schedule>();


        /// Menu categories (e.g., Kabobs, Beverages, Sides) with display order.
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();


        /// Individual menu items (e.g., Chicken Kabob, Pepsi, Qorma).
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();

        /// Customer orders (header row: customer, total, status, etc.).
        public DbSet<Order> Orders => Set<Order>();


        /// Line items inside an order (one row per menu item in the order).
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        /// Model configuration hook.
        /// Called by EF Core to build the model (tables, keys, relationships, etc.).
        /// We always call base.OnModelCreating(builder) so Identity is configured,
        /// then add our own FoodTruck-specific configuration.
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Configure Identity (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(builder);

            // ============================
            // Self-referencing MenuCategory
            // (for parent category / sub-category)
            // ============================

            builder.Entity<MenuCategory>()
                .HasOne(c => c.ParentCategory)          // each category has 0 or 1 parent
                .WithMany(c => c.SubCategories)         // each category can have many sub-categories
                .HasForeignKey(c => c.ParentCategoryId) // FK column in the child
                .OnDelete(DeleteBehavior.Restrict);     // avoid cascading delete loops



            // ============================
            // Decimal precision for money values
            // Prevents truncation and addresses EF Core warnings
            // ============================

            builder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(10, 2); // up to 99999999.99

            builder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(10, 2);

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(10, 2);
        }
    }
}