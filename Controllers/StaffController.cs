using System.Linq;
using System.Threading.Tasks;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{

    /// MVC controller for staff / kitchen operations.
    /// Staff can:
    ///  - View the active order queue
    ///  - Optionally see recent order history
    /// 
    /// NOTE:
    ///  Actual status changes can be done via AJAX calls to
    ///  OrdersApiController or AdminApiController.

    [Authorize(Roles = "Staff,Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;

    
        /// Injects the EF Core DbContext so we can query Orders, etc.
    
        public StaffController(ApplicationDbContext context)
        {
            _context = context;
        }

    
        /// Default landing page for /Staff.
        /// For convenience we just redirect to the OrderQueue page.
    
        public IActionResult Index()
        {
            return RedirectToAction(nameof(OrderQueue));
        }

    
        /// Shows the current order queue for the kitchen.
        /// This page can be backed by AJAX that calls:
        ///   - GET  /api/AdminApi/orders?status=Pending
        ///   - PATCH /api/OrdersApi/{id}/status
        /// 
        /// For now we simply pass a list of active orders to the view.
    
        [HttpGet]
        public async Task<IActionResult> OrderQueue()
        {
            // "Active" orders: not yet completed or permanently closed.
            var activeStatuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.Accepted,
                OrderStatus.Preparing,
                OrderStatus.Ready,

            };

            var orders = await _context.Orders
                .Where(o => activeStatuses.Contains(o.Status))
                .OrderBy(o => o.CreatedAt)         // oldest first so staff see the next order to work on
                .ToListAsync();

            return View(orders); // expects Views/Staff/OrderQueue.cshtml
        }

    
        /// Optional: shows the most recent orders for staff reference.
        /// This is separate from the "My Orders" customer view.
    
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(100)
                .ToListAsync();

            return View(orders); // expects Views/Staff/History.cshtml (optional)
        }

        /// <summary>
        /// Shows details of a specific order
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        /// <summary>
        /// Shows the edit form for an order (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        /// <summary>
        /// Saves changes to an order (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, Order order)
        {
            if (id != order.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order updated successfully.";
                    return RedirectToAction(nameof(OrderQueue));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Orders.AnyAsync(o => o.Id == id))
                        return NotFound();
                    throw;
                }
            }

            // If validation failed, reload the items
            var reloadedOrder = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (reloadedOrder == null) return NotFound();
            return View(reloadedOrder);
        }
    }
}