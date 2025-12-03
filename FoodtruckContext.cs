using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web;

public partial class FoodtruckContext : DbContext
{
    public FoodtruckContext()
    {
    }

    public FoodtruckContext(DbContextOptions<FoodtruckContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=foodtruck.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
