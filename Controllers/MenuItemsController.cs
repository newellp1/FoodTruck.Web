using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Admin CRUD for menu items.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MenuItems
        public async Task<IActionResult> Index()
        {
            var items = _context.MenuItems
                .Include(m => m.MenuCategory)
                .OrderBy(m => m.MenuCategory.DisplayOrder)
                .ThenBy(m => m.Name);
            return View(await items.ToListAsync());
        }

        // GET: MenuItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // GET: MenuItems/Create
        public IActionResult Create()
        {
            ViewData["MenuCategoryId"] = new SelectList(_context.MenuCategories.OrderBy(c => c.DisplayOrder), "Id", "Name");
            return View();
        }

        // POST: MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,IsAvailable,MenuCategoryId")] MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MenuCategoryId"] = new SelectList(_context.MenuCategories.OrderBy(c => c.DisplayOrder), "Id", "Name", menuItem.MenuCategoryId);
            return View(menuItem);
        }

        // GET: MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            ViewData["MenuCategoryId"] = new SelectList(_context.MenuCategories.OrderBy(c => c.DisplayOrder), "Id", "Name", menuItem.MenuCategoryId);
            return View(menuItem);
        }

        // POST: MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,IsAvailable,MenuCategoryId")] MenuItem menuItem)
        {
            if (id != menuItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MenuCategoryId"] = new SelectList(_context.MenuCategories.OrderBy(c => c.DisplayOrder), "Id", "Name", menuItem.MenuCategoryId);
            return View(menuItem);
        }

        // GET: MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}