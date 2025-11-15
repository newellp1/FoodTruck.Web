using FoodTruck.Web.Data;
using FoodTruck.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers.Api
{
    /// <summary>
    /// REST API for order status polling and updates.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetStatus(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            return Ok(new
            {
                order.Id,
                order.Status,
                order.PickupEta,
                order.CancelReason
            });
        }

        // Example of staff status updates
        [Authorize(Roles = "Staff,Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            // TODO: validate allowed transitions
            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(order);
        }
    }
}