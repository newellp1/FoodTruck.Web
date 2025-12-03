using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// Admin CRUD operations for menu categories.
    /// Supports parent/sub-category structure.
    [Authorize(Roles = "Admin")]
    public class MenuCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }


        /// GET: /MenuCategories
        /// Lists all categories, showing their parent category if any.
        public async Task<IActionResult> Index()
        {
            var categories = await _context.MenuCategories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return View(categories);
        }


        /// GET: /MenuCategories/Details/5
        /// Shows details for a single category, including its parent.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuCategory == null) return NotFound();

            return View(menuCategory);
        }


        /// GET: /MenuCategories/Create
        /// Shows the form to create a new category (optionally as a sub-category).
        public IActionResult Create()
        {
            // Parent category dropdown: all categories sorted by display order, then name.
            ViewData["ParentCategoryId"] = new SelectList(
                _context.MenuCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name),
                "Id",
                "Name"
            );

            return View();
        }


        /// POST: /MenuCategories/Create
        /// Creates a new category, including optional ParentCategoryId.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,Description,DisplayOrder,ParentCategoryId")]
            MenuCategory menuCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menuCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, repopulate the parent dropdown
            ViewData["ParentCategoryId"] = new SelectList(
                _context.MenuCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name),
                "Id",
                "Name",
                menuCategory?.ParentCategoryId
            );

            return View(menuCategory);
        }


        /// GET: /MenuCategories/Edit/5
        /// Loads an existing category for editing.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories.FindAsync(id);
            if (menuCategory == null) return NotFound();

            // Parent category dropdown: exclude this category itself to avoid circular parent.
            var possibleParents = await _context.MenuCategories
                .Where(c => c.Id != menuCategory.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewData["ParentCategoryId"] = new SelectList(
                possibleParents,
                "Id",
                "Name",
                menuCategory.ParentCategoryId
            );

            return View(menuCategory);
        }


        /// POST: /MenuCategories/Edit/5
        /// Saves changes to an existing category.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,Description,DisplayOrder,ParentCategoryId")]
            MenuCategory menuCategory)
        {
            if (id != menuCategory.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuCategoryExists(menuCategory.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, repopulate parent dropdown
            var possibleParents = await _context.MenuCategories
                .Where(c => c.Id != menuCategory.Id)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            ViewData["ParentCategoryId"] = new SelectList(
                possibleParents,
                "Id",
                "Name",
                menuCategory.ParentCategoryId
            );

            return View(menuCategory);
        }

        /// GET: /MenuCategories/Delete/5
        /// Shows confirmation page before deleting a category.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuCategory == null) return NotFound();

            return View(menuCategory);
        }

        /// POST: /MenuCategories/Delete/5
        /// Actually deletes the category.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuCategory = await _context.MenuCategories.FindAsync(id);

            if (menuCategory != null)
            {
                _context.MenuCategories.Remove(menuCategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /// Helper: does a MenuCategory with this Id exist?
        private bool MenuCategoryExists(int id)
        {
            return _context.MenuCategories.Any(e => e.Id == id);
        }
    }
}