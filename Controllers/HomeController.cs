using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;

namespace FoodTruck.Web.Controllers
{

    /// Main controller for handling public-facing pages such as Home, About, and Privacy.
    /// Provides schedule lookups to determine current and upcoming food truck locations.
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        /// Initializes the HomeController with logging and database context dependencies.
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// Displays the main homepage.
        /// Retrieves the current active schedule (if any) and the next upcoming schedule.
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var active = await _context.Schedules
                .Include(s => s.Location)
                .Where(s => s.StartTime <= now && s.EndTime >= now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            var next = await _context.Schedules
                .Include(s => s.Location)
                .Where(s => s.StartTime > now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            ViewBag.ActiveSchedule = active;
            ViewBag.NextSchedule = next;

            return View();
        }

        /// Displays the About page.
        /// Retrieves the current active schedule (if any).
        public async Task<IActionResult> About()
        {
            var now = DateTime.Now;

            var active = await _context.Schedules
                .Include(s => s.Location)
                .Where(s => s.StartTime <= now && s.EndTime >= now)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            ViewBag.ActiveSchedule = active;

            return View();
        }

        /// Displays the Privacy page.
        public IActionResult Privacy()
        {
            return View();
        }

        /// Displays the Error page with request ID information.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}