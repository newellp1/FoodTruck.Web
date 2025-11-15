using FoodTruck.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Home and landing pages.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Landing page: shows active or next schedule.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;

            var activeSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.IsActive && s.StartTime <= now && s.EndTime >= now);

            var nextSchedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            ViewBag.ActiveSchedule = activeSchedule;
            ViewBag.NextSchedule = nextSchedule;

            return View();
        }
    }
}