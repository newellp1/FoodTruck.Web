using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{

    /// Handles menu browsing and item detail interactions.
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        /// Shows active menu for current schedule.
        /// Supports sub-categories and their items.
        public async Task<IActionResult> Index(string? search, List<int>? categories)
        {
            var now = DateTime.Now;

            // -------- Active & Next schedule ----------------------
            // Show any schedule within the current time range, prioritize those marked IsActive
            var activeSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime <= now && s.EndTime >= now)
                .OrderByDescending(s => s.IsActive)
                .ThenByDescending(s => s.StartTime)
                .FirstOrDefaultAsync();

            var nextSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();


            // ==========================================================
            // Load *top-level* categories (ParentCategoryId == null)
            // Include:
            // - direct items
            // - subcategories
            // - subcategory items
            // ==========================================================

            var categoriesQuery = _context.MenuCategories
                .Where(c => c.ParentCategoryId == null)   // top-level only
                .Include(c => c.MenuItems.Where(mi => mi.IsAvailable))
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.MenuItems.Where(mi => mi.IsAvailable))
                .OrderBy(c => c.DisplayOrder)
                .AsQueryable();


            // -------- Search ---------------------------------------
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();

                categoriesQuery = categoriesQuery
                    .Select(c => new MenuCategory
                    {
                        Id = c.Id,
                        Name = c.Name,
                        DisplayOrder = c.DisplayOrder,

                        // Filter direct items
                        MenuItems = c.MenuItems
                            .Where(mi => mi.Name.ToLower().Contains(s) ||
                                         (mi.Description ?? "").ToLower().Contains(s))
                            .ToList(),

                        // Filter subcategory items
                        SubCategories = c.SubCategories.Select(sc => new MenuCategory
                        {
                            Id = sc.Id,
                            Name = sc.Name,
                            DisplayOrder = sc.DisplayOrder,
                            ParentCategoryId = sc.ParentCategoryId,
                            MenuItems = sc.MenuItems
                                .Where(mi => mi.Name.ToLower().Contains(s) ||
                                             (mi.Description ?? "").ToLower().Contains(s))
                                .ToList()
                        }).ToList()
                    })
                    .OrderBy(c => c.DisplayOrder);
            }

            // -------- Filter by selected categories -------------------
            if (categories != null && categories.Any())
            {
                categoriesQuery = categoriesQuery.Where(c =>
                    categories.Contains(c.Id) ||
                    c.SubCategories.Any(sc => categories.Contains(sc.Id))
                );
            }

            // -------- Build ViewModel -------------------------------
            var vm = new MenuViewModel
            {
                ActiveSchedule = activeSchedule,
                NextSchedule = activeSchedule == null ? nextSchedule : null,
                Categories = await categoriesQuery.ToListAsync(),
                SearchTerm = search,
                SelectedCategoryIds = categories ?? new List<int>()
            };

            return View(vm);
        }


    
        /// Returns item detail partial for modal (AJAX).    
        public async Task<IActionResult> ItemDetail(int id)
        {
            var item = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.IsAvailable);

            if (item == null)
            {
                return NotFound();
            }

            return PartialView("_ItemDetailModalPartial", item);
        }
    }
}