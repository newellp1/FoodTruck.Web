using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{
    /// <summary>
    /// Public endpoints for menu data (used by AJAX/UI).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveMenu()
        {
            var categories = await _context.MenuCategories
                .Include(c => c.MenuItems.Where(mi => mi.IsAvailable))
                    .ThenInclude(mi => mi.MenuItemModifiers)
                        .ThenInclude(mim => mim.Modifier)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _context.MenuItems
                .Include(mi => mi.MenuItemModifiers)
                    .ThenInclude(mim => mim.Modifier)
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.IsAvailable);

            if (item == null) return NotFound();
            return Ok(item);
        }
    }
}