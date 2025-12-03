using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Admin-only controller that provides:
    ///  - A stats dashboard (Index)
    ///  - A card-based admin home page (Dashboard)
    ///  - Quick redirects to CRUD controllers
    ///  - A Users page listing all users and their roles
    /// </summary>
    [Authorize(Roles = "Admin")] // Only Admins can access anything in this controller
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Constructor that receives the EF Core DbContext and UserManager
        /// via dependency injection.
        /// </summary>
        /// <param name="context">ApplicationDbContext used to query domain data.</param>
        /// <param name="userManager">UserManager used to query Identity users and their roles.</param>
        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // 1) Stats Dashboard (Index)
        // ============================================================

        /// <summary>
        /// Admin stats dashboard.
        /// Shows summary statistics such as:
        ///  - Total orders
        ///  - Pending / InProgress orders
        ///  - Count of menu items, categories, schedules, users
        ///
        /// View: Views/Admin/Index.cshtml (model: Admin)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Count orders and group by status
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var inProgressOrders = await _context.Orders.CountAsync(o => 
                o.Status == OrderStatus.Preparing ||
                o.Status == OrderStatus.Accepted ||
                o.Status == OrderStatus.Preparing ||
                o.Status == OrderStatus.Ready ||
                o.Status == OrderStatus.Completed ||
                o.Status == OrderStatus.Cancelled);

            // Count menu-related entities
            var menuItemCount = await _context.MenuItems.CountAsync();
            var categoryCount = await _context.MenuCategories.CountAsync();

            // Count schedules
            var scheduleCount = await _context.Schedules.CountAsync();

            // Count registered users
            var userCount = await _userManager.Users.CountAsync();

            // Build the Admin stats view model
            var vm = new Admin
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                InProgressOrders = inProgressOrders,
                MenuItemCount = menuItemCount,
                CategoryCount = categoryCount,
                ScheduleCount = scheduleCount,
                UserCount = userCount,
                GeneratedAtUtc = DateTime.UtcNow
            };

            return View(vm);
        }

        // ============================================================
        // 2) Card-based Admin Home (Dashboard)
        // ============================================================

        public IActionResult Dashboard()
        {
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // 3) Users & Roles Listing
        // ============================================================
        public async Task<IActionResult> Users()
        {
            // Load all users from the Identity store
            var users = await _userManager.Users.ToListAsync();

            var vmList = new List<AdminUserViewModel>();

            // Build a simple view model for each user
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                vmList.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName,
                    Roles = roles
                });
            }

            return View(vmList);
        }

        // ============================================================
        // 4) Quick Redirect Helpers to CRUD Pages
        //    (Optional, but handy for the Admin UI)
        // ============================================================

        /// <summary>
        /// Redirects to the MenuCategories management page.
        /// </summary>
        public IActionResult ManageCategories()
        {
            return RedirectToAction("Index", "MenuCategories");
        }

        /// <summary>
        /// Redirects to the MenuItems management page.
        /// </summary>
        public IActionResult ManageMenuItems()
        {
            return RedirectToAction("Index", "MenuItems");
        }

        /// <summary>
        /// Redirects to the Schedules management page.
        /// </summary>
        public IActionResult ManageSchedules()
        {
            return RedirectToAction("Index", "Schedules");
        }

    }
}