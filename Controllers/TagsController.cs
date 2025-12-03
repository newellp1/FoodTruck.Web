using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Manages tags that can be applied to menu items for filtering and categorization.
    /// Tags include dietary preferences (Vegetarian, Gluten-Free) and attributes (Spicy, Popular, New).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display list of all tags ordered by DisplayOrder then Name.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var tags = await _context.Tags
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return View(tags);
        }

        /// <summary>
        /// Display details for a specific tag including all menu items that have this tag.
        /// </summary>
        /// <param name="id">Tag ID</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tag = await _context.Tags
                .Include(t => t.MenuItemTags)
                    .ThenInclude(mt => mt.MenuItem)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null) return NotFound();

            return View(tag);
        }

        /// <summary>
        /// Display form to create a new tag.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Process form submission to create a new tag.
        /// </summary>
        /// <param name="tag">Tag model from form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Tag '{tag.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        /// <summary>
        /// Display form to edit an existing tag.
        /// </summary>
        /// <param name="id">Tag ID</param>
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            return View(tag);
        }

        /// <summary>
        /// Process form submission to update an existing tag.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <param name="tag">Updated tag model from form</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tag tag)
        {
            if (id != tag.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Tag '{tag.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(tag);
        }

        /// <summary>
        /// Display confirmation page before deleting a tag.
        /// </summary>
        /// <param name="id">Tag ID</param>
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tag = await _context.Tags
                .Include(t => t.MenuItemTags)
                    .ThenInclude(mt => mt.MenuItem)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tag == null) return NotFound();

            return View(tag);
        }

        /// <summary>
        /// Process tag deletion. Removes all MenuItemTag associations before deleting the tag.
        /// </summary>
        /// <param name="id">Tag ID</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Tag '{tag.Name}' deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Check if a tag exists in the database.
        /// </summary>
        /// <param name="id">Tag ID</param>
        /// <returns>True if tag exists, false otherwise</returns>
        private bool TagExists(int id)
        {
            return _context.Tags.Any(t => t.Id == id);
        }
    }
}
