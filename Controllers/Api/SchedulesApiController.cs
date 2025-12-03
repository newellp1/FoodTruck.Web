using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{

    /// Public endpoints for schedule information.
    /// Used by the home page and menu page to show
    /// where/when the truck is operating.

    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchedulesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        /// Returns information about the *current* or *next* schedule.
        ///
        /// GET: /api/SchedulesApi/active
        ///
        /// Response shape:
        /// {
        ///   "state": "active" | "upcoming" | "none",
        ///   "schedule": {
        ///       "id": 1,
        ///       "startTime": "...",
        ///       "endTime": "...",
        ///       "truck": { ... },
        ///       "location": { ... }
        ///   } | null
        /// }
    
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var now = DateTime.Now;

            // Try to find an active schedule for "right now".
            var active = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s =>
                    s.IsActive &&
                    s.StartTime <= now &&
                    s.EndTime >= now);

            if (active != null)
            {
                // Truck is currently serving.
                return Ok(new
                {
                    state = "active",
                    schedule = active
                });
            }

            // Otherwise, look for the next upcoming schedule in the future.
            var next = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                // There is a future schedule, but not currently open.
                return Ok(new
                {
                    state = "upcoming",
                    schedule = next
                });
            }

            // No schedules at all (useful so the UI can show "Check back later").
            return Ok(new
            {
                state = "none",
                schedule = (object?)null
            });
        }
    }
}