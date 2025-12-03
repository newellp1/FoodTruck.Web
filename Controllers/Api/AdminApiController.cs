using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{

    /// RESTful API endpoints for Admin/Staff use.
    /// - Manage orders (status updates)
    /// - CRUD for menu categories, items, and schedules.
    /// This supports AJAX-driven back office screens and the
    /// "Admin/Staff API (secured)" requirement.

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class AdminApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // 1) ORDER STATUS MANAGEMENT (for kitchen / staff)
        // =========================================================

    
        /// Lists recent orders for the kitchen/staff dashboard.
        /// You can call this from an AJAX-powered order queue page.
        /// GET: /api/AdminApi/orders?status=Pending
    
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
            [FromQuery] OrderStatus? status = null)
        {
            // Start with all orders, newest first.
            var query = _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            // Optional filter by status.
            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            // Limit to 50 most recent for dashboard.
            var orders = await query.Take(50).ToListAsync();
            return Ok(orders);
        }

    
        /// DTO used when updating an order's status via PATCH.
        /// Example JSON:
        /// {
        ///   "status": "Ready",
        ///   "cancelReason": "Out of stock"
        /// }
    
        public class UpdateOrderStatusRequest
        {
            public OrderStatus Status { get; set; }
            public string? CancelReason { get; set; }
        }

    
        /// Updates the status of an order.
        /// PATCH: /api/AdminApi/orders/{id}/status
        /// Body: { "status": "InProgress", "cancelReason": null }
    
        [HttpPatch("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Set new status.
            order.Status = request.Status;

            // If cancelled/rejected, store the reason.
            if (request.Status == OrderStatus.Cancelled ||
                request.Status == OrderStatus.Rejected)
            {
                order.CancelReason = request.CancelReason;
            }

            // NOTE: We removed "order.LastUpdatedAt = ..." because your Order
            // model does not have that property. If you later add a DateTime
            // LastUpdatedAt property, we can set it here.

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================================================
        // 2) MENU CATEGORY CRUD (Admin/Staff)
        // =========================================================

    
        /// GET: /api/AdminApi/categories
        /// Returns all menu categories including parent relationships.
    
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<MenuCategory>>> GetCategories()
        {
            var categories = await _context.MenuCategories
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }

    
        /// GET: /api/AdminApi/categories/{id}
        /// Returns a single menu category.
    
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<MenuCategory>> GetCategory(int id)
        {
            var category = await _context.MenuCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

    
        /// POST: /api/AdminApi/categories
        /// Creates a new menu category (optionally a sub-category).
    
        [HttpPost("categories")]
        public async Task<ActionResult<MenuCategory>> CreateCategory(MenuCategory category)
        {
            _context.MenuCategories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

    
        /// PUT: /api/AdminApi/categories/{id}
        /// Updates an existing category.
    
        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, MenuCategory category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuCategoryExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

    
        /// DELETE: /api/AdminApi/categories/{id}
        /// Deletes a category.
    
        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.MenuCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.MenuCategories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool MenuCategoryExists(int id) =>
            _context.MenuCategories.Any(e => e.Id == id);

        // =========================================================
        // 3) MENU ITEM CRUD (Admin/Staff)
        // =========================================================

    
        /// GET: /api/AdminApi/menu-items
        /// Returns all menu items with their category.
    
        [HttpGet("menu-items")]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
        {
            var items = await _context.MenuItems
                .Include(mi => mi.MenuCategory)
                .OrderBy(mi => mi.MenuCategory!.DisplayOrder)
                .ThenBy(mi => mi.Name)
                .ToListAsync();

            return Ok(items);
        }

    
        /// GET: /api/AdminApi/menu-items/{id}
        /// Returns a single menu item.
    
        [HttpGet("menu-items/{id}")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
        {
            var item = await _context.MenuItems
                .Include(mi => mi.MenuCategory)
                .FirstOrDefaultAsync(mi => mi.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

    
        /// POST: /api/AdminApi/menu-items
        /// Creates a new menu item.
    
        [HttpPost("menu-items")]
        public async Task<ActionResult<MenuItem>> CreateMenuItem(MenuItem item)
        {
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenuItem), new { id = item.Id }, item);
        }

    
        /// PUT: /api/AdminApi/menu-items/{id}
        /// Updates an existing menu item.
    
        [HttpPut("menu-items/{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, MenuItem item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            _context.Entry(item).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuItemExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

    
        /// DELETE: /api/AdminApi/menu-items/{id}
        /// Deletes a menu item.
    
        [HttpDelete("menu-items/{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool MenuItemExists(int id) =>
            _context.MenuItems.Any(e => e.Id == id);


        // =========================================================
        // 5) SCHEDULE CRUD (Admin/Staff)
        // =========================================================

    
        /// GET: /api/AdminApi/schedules
        /// Returns truck schedules with truck + location info.
    
        [HttpGet("schedules")]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetSchedules()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return Ok(schedules);
        }

    
        /// GET: /api/AdminApi/schedules/{id}
        /// Returns a single schedule.
    
        [HttpGet("schedules/{id}")]
        public async Task<ActionResult<Schedule>> GetSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Truck)
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return Ok(schedule);
        }

    
        /// POST: /api/AdminApi/schedules
        /// Creates a new schedule for a truck/location.
    
        [HttpPost("schedules")]
        public async Task<ActionResult<Schedule>> CreateSchedule(Schedule schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
        }

    
        /// PUT: /api/AdminApi/schedules/{id}
        /// Updates an existing schedule.
    
        [HttpPut("schedules/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return BadRequest();
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduleExists(id))
                    return NotFound();

                throw;
            }

            return NoContent();
        }

    
        /// DELETE: /api/AdminApi/schedules/{id}
        /// Deletes a schedule.
    
        [HttpDelete("schedules/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ScheduleExists(int id) =>
            _context.Schedules.Any(e => e.Id == id);
    }
}