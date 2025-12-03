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

        // =========================================================
        // INDEX – list all items (available + unavailable)
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var items = await _context.MenuItems
                .Include(m => m.MenuCategory)
                    .ThenInclude(c => c.ParentCategory)
                .OrderBy(m => m.MenuCategory.DisplayOrder)
                .ThenBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();

            return View(items);
        }

        // =========================================================
        // DETAILS – view a single item
        // =========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuCategory)
                    .ThenInclude(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // =========================================================
        // CREATE – GET
        // =========================================================
        public IActionResult Create()
        {
            PopulateCategorySelectList();
            return View();
        }

        // =========================================================
        // CREATE – POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,Price,IsAvailable,MenuCategoryId,ImagePath,DisplayOrder")]
            MenuItem menuItem)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList(menuItem.MenuCategoryId);
                return View(menuItem);
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT – GET
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            PopulateCategorySelectList(menuItem.MenuCategoryId);
            return View(menuItem);
        }

        // =========================================================
        // EDIT – POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,Description,Price,IsAvailable,MenuCategoryId,ImagePath,DisplayOrder")]
            MenuItem menuItem)
        {
            if (id != menuItem.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList(menuItem.MenuCategoryId);
                return View(menuItem);
            }

            try
            {
                _context.Update(menuItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(menuItem.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE – GET
        // =========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuCategory)
                    .ThenInclude(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // =========================================================
        // DELETE – POST
        // =========================================================
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

        // =========================================================
        // Helpers
        // =========================================================
        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }

        /// <summary>
        /// Fill ViewData["MenuCategoryId"] with a SelectList for the dropdown.
        /// </summary>
        private void PopulateCategorySelectList(int? selectedId = null)
        {
            var categories = _context.MenuCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToList();

            ViewData["MenuCategoryId"] = new SelectList(
                categories,
                "Id",
                "Name",
                selectedId
            );
        }
    }
}