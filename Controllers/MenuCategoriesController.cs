using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Admin CRUD operations for menu categories.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class MenuCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MenuCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.MenuCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        // GET: MenuCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menuCategory == null) return NotFound();

            return View(menuCategory);
        }

        // GET: MenuCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MenuCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,DisplayOrder")] MenuCategory menuCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menuCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(menuCategory);
        }

        // GET: MenuCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories.FindAsync(id);
            if (menuCategory == null) return NotFound();

            return View(menuCategory);
        }

        // POST: MenuCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DisplayOrder")] MenuCategory menuCategory)
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
            return View(menuCategory);
        }

        // GET: MenuCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuCategory = await _context.MenuCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menuCategory == null) return NotFound();

            return View(menuCategory);
        }

        // POST: MenuCategories/Delete/5
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

        private bool MenuCategoryExists(int id)
        {
            return _context.MenuCategories.Any(e => e.Id == id);
        }
    }
}