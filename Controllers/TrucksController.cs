using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// Admin-only CRUD for trucks (used in schedules).
    [Authorize(Roles = "Admin")]
    public class TrucksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrucksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Trucks
        // Shows a list of all trucks.
        public async Task<IActionResult> Index()
        {
            var trucks = await _context.Trucks
                .OrderBy(t => t.Name)
                .ToListAsync();

            return View(trucks);
        }

        // GET: /Trucks/Details/5
        // Shows details of a single truck.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var truck = await _context.Trucks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (truck == null) return NotFound();

            return View(truck);
        }

        // GET: /Trucks/Create
        // Shows the create truck form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Trucks/Create
        // Handles form submission to create a new truck.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,LicensePlate,IsActive")] Truck truck)
        {
            if (ModelState.IsValid)
            {
                _context.Trucks.Add(truck);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, redisplay form with validation messages.
            return View(truck);
        }

        // GET: /Trucks/Edit/5
        // Shows the edit form for an existing truck.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var truck = await _context.Trucks.FindAsync(id);
            if (truck == null) return NotFound();

            return View(truck);
        }

        // POST: /Trucks/Edit/5
        // Handles form submission to update an existing truck.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LicensePlate,IsActive")] Truck truck)
        {
            if (id != truck.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(truck);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TruckExists(truck.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(truck);
        }

        // GET: /Trucks/Delete/5
        // Shows a confirmation page for deleting a truck.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var truck = await _context.Trucks
                .FirstOrDefaultAsync(t => t.Id == id);

            if (truck == null) return NotFound();

            return View(truck);
        }

        // POST: /Trucks/Delete/5
        // Actually deletes the truck after confirmation.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var truck = await _context.Trucks.FindAsync(id);
            if (truck != null)
            {
                try
                {
                    _context.Trucks.Remove(truck);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // If there are related schedules, deletion may fail due to FK constraints.
                    // You can show a friendlier message or redirect with TempData if you like.
                    ModelState.AddModelError(string.Empty,
                        "Unable to delete this truck because it is used in one or more schedules.");
                    return View(truck);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper to check existence
        private bool TruckExists(int id)
        {
            return _context.Trucks.Any(e => e.Id == id);
        }
    }
}