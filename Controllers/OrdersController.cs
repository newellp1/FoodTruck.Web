using System.Security.Claims;
using System.Text.Json;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Handles checkout, order confirmation, tracking, and history.
    /// </summary>
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Must match the key used in CartController
        private const string CartSessionKey = "Cart";

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================
        // Cart helpers (same logic as CartController)
        // ============================

        private CartViewModel GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new CartViewModel();
            }

            try
            {
                var cart = JsonSerializer.Deserialize<CartViewModel>(json);
                return cart ?? new CartViewModel();
            }
            catch
            {
                // If deserialization fails, just start with an empty cart
                return new CartViewModel();
            }
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        // ============================
        // Checkout (GET)
        // ============================

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();

            // If somehow the cart is empty, go back to cart page
            if (cart.Items == null || !cart.Items.Any())
            {
                TempData["CartMessage"] = "Your cart is empty. Add some items before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutViewModel();

            // Pre-fill contact name for logged-in users
            if (User.Identity?.IsAuthenticated == true)
            {
                vm.ContactName = User.Identity.Name ?? string.Empty;
            }

            return View(vm);
        }

        // ============================
        // Checkout (POST) – place order
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = GetCartFromSession();

        if (cart.Items == null || !cart.Items.Any())
        {
            ModelState.AddModelError(string.Empty, "Your cart is empty. Add items before placing an order.");
        }

        // Extra validation when paying by card (no real payment is processed)
        if (model.PaymentMethod == "Card")
        {
            if (string.IsNullOrWhiteSpace(model.CardNumber))
            {
                ModelState.AddModelError(nameof(model.CardNumber),
                    "Card number is required for card payments.");
            }

            if (string.IsNullOrWhiteSpace(model.Expiry))
            {
                ModelState.AddModelError(nameof(model.Expiry),
                    "Expiry is required for card payments.");
            }

            if (string.IsNullOrWhiteSpace(model.Cvv))
            {
                ModelState.AddModelError(nameof(model.Cvv),
                    "CVV is required for card payments.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Revalidate items & recalc totals from the database
        decimal total = 0;

        foreach (var line in cart.Items ?? new List<CartItemViewModel>())
        {
            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.Id == line.MenuItemId && mi.IsAvailable);

            if (menuItem == null)
            {
                ModelState.AddModelError(string.Empty, $"Item '{line.Name ?? "Unknown"}' is no longer available.");
                return View(model);
            }

            total += line.LineTotal;
        }

        // Create the order record (we do NOT store card details anywhere)
        var order = new Order
        {
            ContactName = model.ContactName,
            ContactPhone = model.ContactPhone,
            ContactEmail = model.ContactEmail,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Total = total,
            PickupEta = DateTime.UtcNow.AddMinutes(15)
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            order.CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create OrderItems
        foreach (var line in cart.Items ?? new List<CartItemViewModel>())
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = line.MenuItemId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Notes = line.Notes
            };

            _context.OrderItems.Add(orderItem);

        }

        await _context.SaveChangesAsync();

        // Clear cart after success
        ClearCart();

        return RedirectToAction(nameof(Confirm), new { id = order.Id });
    }

        // ============================
        // Confirm & Track
        // ============================

        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (order == null) return NotFound();

            var vm = new OrderTrackingViewModel
            {
                OrderId = order.Id,
                Status = order.Status,
                PickupEta = order.PickupEta,
                CancelReason = order.CancelReason,
                OrderTotal = order.Total,
                CanCustomerCancel = order.Status == OrderStatus.Pending ||
                                    order.Status == OrderStatus.Accepted,
                ItemSummaries = order.Items.Select(oi => 
                    $"{oi.Quantity}x {oi.MenuItem.Name} - {(oi.UnitPrice * oi.Quantity):C}").ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Track(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var vm = new OrderTrackingViewModel
            {
                OrderId = order.Id,
                Status = order.Status,
                PickupEta = order.PickupEta,
                CancelReason = order.CancelReason,
                CanCustomerCancel = order.Status == OrderStatus.Pending ||
                                    order.Status == OrderStatus.Accepted,
            };

            return View(vm);
        }


        // ============================
        // Public "Find My Order" screen
        // ============================

        [HttpGet]
        public IActionResult Find()
        {
            // Simple search form
            return View(new OrderLookupViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Find(OrderLookupViewModel model)
        {
            if (model.OrderId == null &&
                (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Phone)))
            {
                ModelState.AddModelError(string.Empty,
                    "Enter an Order #, or Full Name and Phone Number.");
                return View(model);
            }

            Order? order = null;

            if (model.OrderId.HasValue)
            {
                order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == model.OrderId.Value);
            }
            else
            {
                var name = model.FullName!.Trim();
                var phone = model.Phone!.Trim();

                // Find most recent order by this name + phone
                order = await _context.Orders
                    .Where(o => o.ContactName == name && o.ContactPhone == phone)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            if (order == null)
            {
                ModelState.AddModelError(string.Empty, "No order found with that information.");
                return View(model);
            }

            // Redirect to existing Track action (status page)
            return RedirectToAction(nameof(Track), new { id = order.Id });
        }

        // ============================
        // Order history for logged-in customer
        // ============================

        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }
        
        // ============================
        // Admin: list all orders (with optional status filter)
        // ============================

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? status)
        {
            // Start with all orders
            var query = _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            // Optional status filter from query string: ?status=Pending or ?status=Preparing
            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var parsedStatus))
            {
                query = query.Where(o => o.Status == parsedStatus);
            }

            var orders = await query.ToListAsync();

            return View(orders);
        }

        // ============================
        // Customer: cancel their own order (Pending or Accepted only)
        // ============================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

            if (order == null) return NotFound();

            if (order.Status == OrderStatus.Pending ||
                order.Status == OrderStatus.Accepted)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancelReason = $"Cancelled by customer at {DateTime.UtcNow.ToLocalTime():g}.";
                await _context.SaveChangesAsync();

                TempData["Message"] = "Your order was cancelled.";
            }
            else
            {
                TempData["Error"] = "You can only cancel orders that are Pending or Accepted.";
            }

            return RedirectToAction(nameof(History));
        }

        // ============================
        // Customer: cancel their own order (Pending or Accepted only)
        // ============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelFromTracking(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (order.Status == OrderStatus.Pending ||
                order.Status == OrderStatus.Accepted)
            {
                order.Status = OrderStatus.Cancelled;
                order.CancelReason = $"Cancelled by customer at {DateTime.UtcNow.ToLocalTime():g}.";
                await _context.SaveChangesAsync();

                TempData["Message"] = "Your order was cancelled.";
            }
            else
            {
                TempData["Error"] = "You can only cancel orders that are Pending or Accepted.";
            }

            return RedirectToAction(nameof(Track), new { id = order.Id });
        }


        // ============================
        // Admin: update order status
        // ============================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var current = order.Status;

            if (!IsValidAdminStatusChange(current, newStatus))
            {
                TempData["Error"] = $"Cannot change status from {current} to {newStatus}.";
                return RedirectToAction(nameof(Index));
            }

            order.Status = newStatus;

            if (newStatus == OrderStatus.Cancelled && string.IsNullOrWhiteSpace(order.CancelReason))
            {
                order.CancelReason = $"Cancelled by admin at {DateTime.UtcNow.ToLocalTime():g}.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool IsValidAdminStatusChange(OrderStatus current, OrderStatus next)
        {
            if (current == next) return true;

            switch (current)
            {
                case OrderStatus.Pending:
                    return next == OrderStatus.Accepted ||
                        next == OrderStatus.Cancelled;

                case OrderStatus.Accepted:
                    return next == OrderStatus.Preparing ||
                        next == OrderStatus.Cancelled;

                case OrderStatus.Preparing:
                    return next == OrderStatus.Ready ||
                        next == OrderStatus.Cancelled;

                case OrderStatus.Ready:
                    return next == OrderStatus.Completed ||
                        next == OrderStatus.Cancelled;

                case OrderStatus.Completed:
                case OrderStatus.Cancelled:
                    return false;

                default:
                    return false;
            }
        }   
    
    }
}

