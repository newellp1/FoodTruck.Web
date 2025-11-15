using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Handles menu browsing and item detail interactions.
    /// </summary>
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Shows active menu for current schedule.
        /// </summary>
        public async Task<IActionResult> Index(string? search, List<int>? categories)
        {
            var now = DateTime.UtcNow;

            var activeSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.IsActive && s.StartTime <= now && s.EndTime >= now);

            var nextSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            var categoriesQuery = _context.MenuCategories
                .Include(c => c.MenuItems.Where(mi => mi.IsAvailable))
                    .ThenInclude(mi => mi.MenuItemModifiers)
                        .ThenInclude(mim => mim.Modifier)
                .OrderBy(c => c.DisplayOrder)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                categoriesQuery = categoriesQuery.Select(c => new MenuCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.DisplayOrder,
                    MenuItems = c.MenuItems
                        .Where(mi => mi.Name.ToLower().Contains(search) ||
                                     (mi.Description ?? "").ToLower().Contains(search))
                        .ToList()
                }).OrderBy(c => c.DisplayOrder);
            }

            if (categories != null && categories.Any())
            {
                categoriesQuery = categoriesQuery.Where(c => categories.Contains(c.Id));
            }

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

        /// <summary>
        /// Returns item detail partial for modal (AJAX).
        /// </summary>
        public async Task<IActionResult> ItemDetail(int id)
        {
            var item = await _context.MenuItems
                .Include(mi => mi.MenuItemModifiers)
                    .ThenInclude(mim => mim.Modifier)
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.IsAvailable);

            if (item == null)
            {
                return NotFound();
            }

            return PartialView("_ItemDetailModalPartial", item);
        }
    }
}