using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{

    /// Admin CRUD for truck schedules.

    [Authorize(Roles = "Admin")]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Schedules
        public async Task<IActionResult> Index()
        {
            var schedules = _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .OrderByDescending(s => s.StartTime);

            return View(await schedules.ToListAsync());
        }

        // GET: Schedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null) return NotFound();

            return View(schedule);
        }

        // GET: Schedules/Create
        public IActionResult Create()
        {
            ViewData["TruckId"] = new SelectList(_context.Trucks, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            return View();
        }

        // POST: Schedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TruckId,LocationId,IsActive")] Schedule schedule)
        {
            // Remove any validation errors for properties we don't need from the form
            ModelState.Remove("StartTime");
            ModelState.Remove("EndTime");
            ModelState.Remove("Truck");
            ModelState.Remove("Location");
            
            if (ModelState.IsValid)
            {
                // Set schedule for all day today
                var today = DateTime.Now.Date;
                schedule.StartTime = today; // 12:00 AM (start of day)
                schedule.EndTime = today.AddDays(1).AddSeconds(-1);   // 11:59:59 PM (end of day)
                
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                
                // Load the truck and location names for the message
                var savedSchedule = await _context.Schedules
                    .Include(s => s.Truck)
                    .Include(s => s.Location)
                    .FirstOrDefaultAsync(s => s.Id == schedule.Id);
                
                if (savedSchedule != null)
                {
                    TempData["SuccessMessage"] = $"Schedule created successfully! {savedSchedule.Truck.Name} is now scheduled at {savedSchedule.Location.Name} for today ({savedSchedule.StartTime:MM/dd/yyyy}).";
                }
                
                return RedirectToAction(nameof(Index));
            }

            ViewData["TruckId"] = new SelectList(_context.Trucks, "Id", "Name", schedule.TruckId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", schedule.LocationId);
            return View(schedule);
        }

        // GET: Schedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();

            ViewData["TruckId"] = new SelectList(_context.Trucks, "Id", "Name", schedule.TruckId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", schedule.LocationId);
            return View(schedule);
        }

        // POST: Schedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TruckId,LocationId,StartTime,EndTime,IsActive")] Schedule schedule)
        {
            if (id != schedule.Id) return NotFound();

            if (schedule.EndTime <= schedule.StartTime)
            {
                ModelState.AddModelError(string.Empty, "End time must be after start time.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["TruckId"] = new SelectList(_context.Trucks, "Id", "Name", schedule.TruckId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", schedule.LocationId);
            return View(schedule);
        }

        // GET: Schedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var schedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (schedule == null) return NotFound();

            return View(schedule);
        }

        // POST: Schedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}