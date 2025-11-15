using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{
    /// <summary>
    /// Public endpoint for active/next schedules.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchedulesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var now = DateTime.UtcNow;

            var active = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.IsActive && s.StartTime <= now && s.EndTime >= now);

            if (active != null) return Ok(active);

            var next = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            return Ok(next);
        }
    }
}