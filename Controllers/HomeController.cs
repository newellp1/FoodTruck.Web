using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;

namespace FoodTruck.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}