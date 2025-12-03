using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Database & EF Core configuration
// ==========================================

// Read connection string from appsettings.json ("DefaultConnection")
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register ApplicationDbContext and configure it to use SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Helpful detailed EF Core exception pages in development
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ==========================================
// 2. Force specific URLs (ports) for Kestrel
//    -> http://localhost:5062
//    -> https://localhost:7106
// ==========================================

builder.WebHost.UseUrls("http://localhost:5062", "https://localhost:7106");

// ==========================================
// 3. Identity (Authentication + Roles)
// ==========================================

builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        // For class project: do NOT require confirmed email to sign in.
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()                          // Enable role support (Admin, Customer, Staff)
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Store Identity data in our DbContext

// ==========================================
// 4. MVC, Razor Pages, and Session
// ==========================================

// Add MVC controllers with views (for Home, Menu, Cart, Orders, Admin, etc.)
builder.Services.AddControllersWithViews();

// Add Razor Pages (needed for Identity UI like /Identity/Account/Login, Register, etc.)
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

// Enable Session state (used to persist Cart between requests)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout (e.g., for Cart)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==========================================
// 5. HTTP request pipeline
// ==========================================

if (app.Environment.IsDevelopment())
{
    // Developer-friendly pages in development
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Redirect HTTP -> HTTPS
app.UseHttpsRedirection();

// Serve static files from wwwroot (CSS, JS, images, etc.)
app.UseStaticFiles();

app.UseRouting();

// Enable Session BEFORE Authentication if session might be used in auth flow
app.UseSession();

// Enable Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// ==========================================
// 6. Seed roles and admin user
// ==========================================

await IdentitySeed.SeedAsync(app);
await DbInitializer.SeedAsync(app);

// ==========================================
// 7. Endpoint routing
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ==========================================
// 8. Run the app
// ==========================================

await app.RunAsync();