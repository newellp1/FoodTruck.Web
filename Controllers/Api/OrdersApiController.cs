using FoodTruck.Web.Data;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{
    /// REST API for order status:
    /// - Customers use GET /{id}/status for live tracking (AJAX polling).
    /// - Staff/Admin use PATCH /{id}/status to advance or cancel orders.
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------------------------------------------------
        // 1) CUSTOMER STATUS POLLING
        // -------------------------------------------------------------

        /// Returns the current status of an order.
        /// Used by the customer tracking page to auto-refresh
        /// status, ETA, and cancel reason.
        /// GET: /api/OrdersApi/{id}/status
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = $"Order {id} not found." });
            }

            return Ok(new
            {
                order.Id,
                order.Status,
                order.PickupEta,
                order.CancelReason
            });
        }

        // -------------------------------------------------------------
        // 2) STAFF / ADMIN STATUS UPDATES
        // -------------------------------------------------------------

        /// DTO for status update requests.
        /// Example JSON:
        /// { "status": "InProgress", "cancelReason": "Out of stock" }
        public class UpdateStatusRequest
        {
            public OrderStatus Status { get; set; }
            public string? CancelReason { get; set; }
        }

        /// Allows Staff/Admin to update the status of an order.
        /// This is intended to be called from an AJAX-driven
        /// kitchen/queue screen.
        /// PATCH: /api/OrdersApi/{id}/status
        /// Body:  { "status": "Ready", "cancelReason": null }
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateStatusRequest request)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound(new { message = $"Order {id} not found." });
            }

            // Prevent changes to already-completed or cancelled orders.
            if (order.Status == OrderStatus.Completed ||
                order.Status == OrderStatus.Cancelled ||
                order.Status == OrderStatus.Rejected)
            {
                return BadRequest(new
                {
                    message = $"Order {id} is already {order.Status} and cannot be changed."
                });
            }

            // OPTIONAL: can add “allowed transitions” rules here
            // (e.g., Pending -> Accepted -> InProgress -> Ready -> Completed).
            // For now, we just set the requested status.

            order.Status = request.Status;

            // If this is a cancel-like status, store the reason.
            if (request.Status == OrderStatus.Cancelled ||
                request.Status == OrderStatus.Rejected)
            {
                order.CancelReason = request.CancelReason;
            }

            await _context.SaveChangesAsync();

            // Return the minimal info the front-end needs.
            return Ok(new
            {
                order.Id,
                order.Status,
                order.PickupEta,
                order.CancelReason
            });
        }
    }
}