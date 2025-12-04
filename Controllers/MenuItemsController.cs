using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// Admin CRUD for menu items.
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
                    .ThenInclude(c => c!.ParentCategory)
                .OrderBy(m => m.MenuCategory!.DisplayOrder)
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
                    .ThenInclude(c => c!.ParentCategory)
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
            PopulateTagsCheckList();
            return View();
        }

        // =========================================================
        // CREATE – POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Description,Price,IsAvailable,MenuCategoryId,ImagePath,DisplayOrder")]
            MenuItem menuItem, int[] selectedTags)
        {
            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList(menuItem.MenuCategoryId);
                PopulateTagsCheckList(selectedTags);
                return View(menuItem);
            }

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            // Add selected tags
            if (selectedTags != null)
            {
                foreach (var tagId in selectedTags)
                {
                    _context.Add(new MenuItemTag { MenuItemId = menuItem.Id, TagId = tagId });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // EDIT – GET
        // =========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.MenuItemTags)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menuItem == null) return NotFound();

            PopulateCategorySelectList(menuItem.MenuCategoryId);
            PopulateTagsCheckList(menuItem.MenuItemTags?.Select(mt => mt.TagId).ToArray());
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
            MenuItem menuItem, int[] selectedTags)
        {
            if (id != menuItem.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList(menuItem.MenuCategoryId);
                PopulateTagsCheckList(selectedTags);
                return View(menuItem);
            }

            try
            {
                _context.Update(menuItem);

                // Update tags - remove old ones and add new ones
                var existingTags = _context.Set<MenuItemTag>().Where(mt => mt.MenuItemId == id);
                _context.RemoveRange(existingTags);

                if (selectedTags != null)
                {
                    foreach (var tagId in selectedTags)
                    {
                        _context.Add(new MenuItemTag { MenuItemId = id, TagId = tagId });
                    }
                }

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
                    .ThenInclude(c => c!.ParentCategory)
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
        // HELPER METHODS
        // =========================================================

        /// Fill ViewData["MenuCategoryId"] with a SelectList for the dropdown.
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

        /// Populate ViewBag.Tags for checkbox list.

        /// <param name="selectedTags">Array of selected tag IDs</param>
        private void PopulateTagsCheckList(int[]? selectedTags = null)
        {
            var allTags = _context.Tags
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name)
                .ToList();

            ViewBag.Tags = allTags;
            ViewBag.SelectedTags = selectedTags ?? Array.Empty<int>();
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}