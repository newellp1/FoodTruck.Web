using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{

    /// Public (no auth required) endpoints for menu data.
    /// Used by your customer-facing UI / AJAX to load menu info.

    [Route("api/[controller]")]
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuApiController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        /// Returns the active menu, grouped by categories and sub-categories.
        /// Only items with IsAvailable = true are included.
        ///
        /// GET: /api/MenuApi/active
    
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMenu()
        {

            var categories = await _context.MenuCategories
                .Where(c => c.ParentCategoryId == null)  // top-level only
                // Top-level category items
                .Include(c => c.MenuItems.Where(mi => mi.IsAvailable))
                // Sub-categories and their items
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.MenuItems.Where(mi => mi.IsAvailable))
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(categories);
        }

    
        /// Returns a single menu item (if it is available),
        /// Used for the "View & Add" detail modal.
        /// GET: /api/MenuApi/item/5
    
        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.MenuItems
                .Include(mi => mi.MenuCategory)
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.IsAvailable);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }
    }
}