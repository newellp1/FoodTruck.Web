using System.Security.Claims;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.Enums;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
        private readonly CartController _cartController; // reuse methods

        public OrdersController(ApplicationDbContext context, CartController cartController)
        {
            _context = context;
            _cartController = cartController;
        }

        private CartViewModel GetCartFromSession()
        {
            // Quick helper to reuse session methods
            var getCartMethod = typeof(CartController)
                .GetMethod("GetCart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (CartViewModel)getCartMethod!.Invoke(_cartController, Array.Empty<object>())!;
        }

        private void ClearCart()
        {
            var saveCartMethod = typeof(CartController)
                .GetMethod("SaveCart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            saveCartMethod!.Invoke(_cartController, new object[] { new CartViewModel() });
        }

        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var vm = new CheckoutViewModel();

            if (User.Identity?.IsAuthenticated == true)
            {
                vm.ContactName = User.Identity.Name ?? string.Empty;
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Cart is empty.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Revalidate items & recalc prices
            decimal total = 0;
            foreach (var line in cart.Items)
            {
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(mi => mi.Id == line.MenuItemId && mi.IsAvailable);

                if (menuItem == null)
                {
                    ModelState.AddModelError(string.Empty, $"Item {line.Name} is no longer available.");
                    return View(model);
                }

                // Optional: recalc unit price & compare with line.UnitPrice
                total += line.LineTotal;
            }

            var order = new Order
            {
                ContactName = model.ContactName,
                ContactPhone = model.ContactPhone,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Total = total
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                order.CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // Very simple ETA: now + 15 minutes
            order.PickupEta = DateTime.UtcNow.AddMinutes(15);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // OrderItems
            foreach (var line in cart.Items)
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
                await _context.SaveChangesAsync();

                foreach (var modId in line.ModifierIds)
                {
                    _context.OrderItemModifiers.Add(new OrderItemModifier
                    {
                        OrderItemId = orderItem.Id,
                        ModifierId = modId
                    });
                }
            }

            await _context.SaveChangesAsync();

            ClearCart();

            return RedirectToAction(nameof(Confirm), new { id = order.Id });
        }

        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderTrackingViewModel
            {
                OrderId = order.Id,
                Status = order.Status,
                PickupEta = order.PickupEta,
                CancelReason = order.CancelReason
            };

            return View(vm);
        }

        public async Task<IActionResult> Track(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var vm = new OrderTrackingViewModel
            {
                OrderId = order.Id,
                Status = order.Status,
                PickupEta = order.PickupEta,
                CancelReason = order.CancelReason
            };

            return View(vm);
        }

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
    }
}