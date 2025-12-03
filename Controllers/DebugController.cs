using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    public class DebugController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Schedules()
        {
            var now = DateTime.Now;
            
            var allSchedules = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.CurrentTime = now;
            ViewBag.CurrentTimeString = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            ViewBag.AllSchedules = allSchedules;
            ViewBag.ScheduleCount = allSchedules.Count;
            
            // Test the same query used in Home and Menu controllers
            var activeSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime <= now && s.EndTime >= now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();
                
            ViewBag.ActiveSchedule = activeSchedule;
            
            return View();
        }
        
        public async Task<IActionResult> CreateTestSchedule()
        {
            var truck = await _context.Trucks.FirstOrDefaultAsync();
            var location = await _context.Locations.FirstOrDefaultAsync();
            
            if (truck == null || location == null)
            {
                return Content("No truck or location found in database");
            }
            
            var today = DateTime.Now.Date;
            var schedule = new Models.Schedule
            {
                TruckId = truck.Id,
                LocationId = location.Id,
                StartTime = today,
                EndTime = today.AddDays(1).AddSeconds(-1),
                IsActive = true
            };
            
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Schedules));
        }
    }
}
