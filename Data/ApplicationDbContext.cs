using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Data
{
    /// <summary>
    /// EF Core DbContext for the application.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Truck> Trucks => Set<Truck>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Schedule> Schedules => Set<Schedule>();
        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Modifier> Modifiers => Set<Modifier>();
        public DbSet<MenuItemModifier> MenuItemModifiers => Set<MenuItemModifier>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderItemModifier> OrderItemModifiers => Set<OrderItemModifier>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MenuItemModifier>()
                .HasKey(mm => new { mm.MenuItemId, mm.ModifierId });

            builder.Entity<MenuItemModifier>()
                .HasOne(mm => mm.MenuItem)
                .WithMany(m => m.MenuItemModifiers)
                .HasForeignKey(mm => mm.MenuItemId);

            builder.Entity<MenuItemModifier>()
                .HasOne(mm => mm.Modifier)
                .WithMany(m => m.MenuItemModifiers)
                .HasForeignKey(mm => mm.ModifierId);

            builder.Entity<OrderItemModifier>()
                .HasKey(oim => new { oim.OrderItemId, oim.ModifierId });

            builder.Entity<OrderItemModifier>()
                .HasOne(oim => oim.OrderItem)
                .WithMany(oi => oi.OrderItemModifiers)
                .HasForeignKey(oim => oim.OrderItemId);

            builder.Entity<OrderItemModifier>()
                .HasOne(oim => oim.Modifier)
                .WithMany()
                .HasForeignKey(oim => oim.ModifierId);
        }
    }
}