using System.Text.Json;
using FoodTruck.Web.Data;
using FoodTruck.Web.Models;
using FoodTruck.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodTruck.Web.Controllers
{
    /// <summary>
    /// Handles cart operations stored in session.
    /// </summary>
    public class CartController : Controller
    {
        private const string CART_KEY = "CART";
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

        /// <summary>
        /// Show cart page.
        /// </summary>
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        /// <summary>
        /// Add line to cart (AJAX).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add(int menuItemId, int quantity, string? notes, List<int>? modifierIds)
        {
            if (quantity <= 0) quantity = 1;

            var menuItem = await _context.MenuItems
                .Include(mi => mi.MenuItemModifiers)
                    .ThenInclude(mim => mim.Modifier)
                .FirstOrDefaultAsync(mi => mi.Id == menuItemId && mi.IsAvailable);

            if (menuItem == null)
            {
                return BadRequest("Item unavailable.");
            }

            modifierIds ??= new List<int>();

            // Validate modifiers: check they belong to this item
            var validModifierIds = menuItem.MenuItemModifiers.Select(mim => mim.ModifierId).ToHashSet();
            if (!modifierIds.All(id => validModifierIds.Contains(id)))
            {
                return BadRequest("Invalid modifiers.");
            }

            // Optional: enforce min/max for modifier Types
            // (skipped for brevity, but this is where you'd check)

            var basePrice = menuItem.Price;
            var modifiers = await _context.Modifiers.Where(m => modifierIds.Contains(m.Id)).ToListAsync();
            var totalDelta = modifiers.Sum(m => m.PriceDelta);
            var unitPrice = basePrice + totalDelta;

            var cart = GetCart();

            // Merge if same config already in cart
            var existing = cart.Items.FirstOrDefault(i =>
                i.MenuItemId == menuItemId &&
                i.Notes == notes &&
                i.ModifierIds.OrderBy(x => x).SequenceEqual(modifierIds.OrderBy(x => x)));

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Notes = notes,
                    ModifierIds = modifierIds,
                    ModifierSummary = string.Join(", ", modifiers.Select(m => $"{m.Name} ({m.PriceDelta:+0.00;-0.00;0})"))
                });
            }

            SaveCart(cart);

            return PartialView("~/Views/Shared/_CartSummaryPartial.cshtml", cart);
        }

        /// <summary>
        /// Update quantity.
        /// </summary>
        [HttpPost]
        public IActionResult UpdateQuantity(int index, int quantity)
        {
            var cart = GetCart();
            if (index < 0 || index >= cart.Items.Count)
            {
                return BadRequest();
            }

            if (quantity <= 0)
            {
                cart.Items.RemoveAt(index);
            }
            else
            {
                cart.Items[index].Quantity = quantity;
            }

            SaveCart(cart);
            return PartialView("~/Views/Cart/_CartLinesPartial.cshtml", cart);
        }

        /// <summary>
        /// Remove line.
        /// </summary>
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

        public IActionResult Clear()
        {
            SaveCart(new CartViewModel());
            return RedirectToAction("Index");
        }
    }
}