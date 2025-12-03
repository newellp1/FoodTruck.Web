using System.Text.Json;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// Handles cart operations stored in session.
    public class CartController : Controller
    {
        // Must match OrdersController.CartSessionKey ("Cart")
        private const string CART_KEY = "Cart";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private CartViewModel GetCart()
        {
            var json = HttpContext.Session.GetString(CART_KEY);
            if (string.IsNullOrEmpty(json))
            {
                return new CartViewModel();
            }

            return JsonSerializer.Deserialize<CartViewModel>(json) ?? new CartViewModel();
        }

        private void SaveCart(CartViewModel cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_KEY, json);
        }

        /// Show cart page.
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        /// Add line to cart (AJAX).
        [HttpPost]
        public async Task<IActionResult> Add(int menuItemId, int quantity, string? notes)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(mi => mi.Id == menuItemId && mi.IsAvailable);

            if (menuItem == null)
            {
                return BadRequest("Item unavailable.");
            }

            var cart = GetCart();

            // See if there's already the same item (same menuItemId + same notes)
            var existing = cart.Items.FirstOrDefault(i =>
                i.MenuItemId == menuItemId &&
                (i.Notes ?? string.Empty) == (notes ?? string.Empty));

            if (existing != null)
            {
                // Just bump the quantity and recalc line total
                existing.Quantity += quantity;

            }
            else
            {
                // Add a brand new line
                cart.Items.Add(new CartItemViewModel
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Quantity = quantity,
                    UnitPrice = menuItem.Price,
                    Notes = notes
                });
            }

            SaveCart(cart);

            // Re-render the cart summary partial
            return PartialView("~/Views/Shared/_CartSummaryPartial.cshtml", cart);
        }

        /// Update quantities or remove a line.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(CartViewModel model, int? removeIndex)
        {
            var cart = GetCart();

            // Remove a line if removeIndex is provided
            if (removeIndex.HasValue)
            {
                int idx = removeIndex.Value;
                if (idx >= 0 && idx < cart.Items.Count)
                {
                    cart.Items.RemoveAt(idx);
                }

                SaveCart(cart);
                return RedirectToAction(nameof(Index));
            }

            // Otherwise, update quantities
            if (model.Items != null && model.Items.Count == cart.Items.Count)
            {
                for (int i = 0; i < cart.Items.Count; i++)
                {
                    var newQty = model.Items[i].Quantity;
                    if (newQty < 1) newQty = 1;

                    cart.Items[i].Quantity = newQty;
                 }
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        /// Remove a line and return updated cart lines partial.
        [HttpPost]
        public IActionResult Remove(int index)
        {
            var cart = GetCart();
            if (index < 0 || index >= cart.Items.Count)
            {
                return BadRequest();
            }

            cart.Items.RemoveAt(index);
            SaveCart(cart);

            return PartialView("~/Views/Cart/_CartLinesPartial.cshtml", cart);
        }

        /// Clear the cart completely.
        public IActionResult Clear()
        {
            SaveCart(new CartViewModel());
            return RedirectToAction("Index");
        }
    }
}