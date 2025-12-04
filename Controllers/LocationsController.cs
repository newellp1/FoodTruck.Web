using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// Admin-only CRUD for locations (used in schedules).
    [Authorize(Roles = "Admin")]
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Locations
        // Shows a list of all locations.
        public async Task<IActionResult> Index()
        {
            var locations = await _context.Locations
                .OrderBy(l => l.Name)
                .ToListAsync();

            return View(locations);
        }

        // GET: /Locations/Details/5
        // Shows details for a single location.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null) return NotFound();

            return View(location);
        }

        // GET: /Locations/Create
        // Shows the create location form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Locations/Create
        // Handles form submission to create a new location.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,Address,City,State,ZipCode,Notes,IsActive")] Location location)
        {
            // Server-side validation of the posted model
            if (!ModelState.IsValid)
            {
                // Re-display the form and show validation messages.
                return View(location);
            }

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            // After successful save, go back to the list
            return RedirectToAction(nameof(Index));
        }

        // GET: /Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();

            return View(location);
        }

        // POST: /Locations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,Address,City,State,ZipCode,Notes,IsActive")] Location location)
        {
            if (id != location.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(location);
            }

            try
            {
                _context.Update(location);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(location.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == id);

            if (location == null) return NotFound();

            return View(location);
        }

        // POST: /Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                try
                {
                    _context.Locations.Remove(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Friendly FK failure message
                    ModelState.AddModelError(string.Empty,
                        "Unable to delete this location because it is used in one or more schedules.");
                    return View(location);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}