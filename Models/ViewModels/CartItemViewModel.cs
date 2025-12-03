using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{
    /// Represents a single item stored in the user's cart session.
    /// Includes quantity, price, and a human-readable summary.
    public class CartItemViewModel
    {
        /// The ID of the MenuItem being purchased.
        public int MenuItemId { get; set; }

        /// Display name of the menu item (e.g., "Coke", "Beef Taco").
        public string Name { get; set; } = string.Empty;

        /// The base unit price of the menu item.
        public decimal UnitPrice { get; set; }

        /// Quantity of this item in the cart.
        public int Quantity { get; set; }

        /// Customer-provided notes (e.g., “no onions”, “extra crispy”).
        public string? Notes { get; set; }

        /// Total price for this line item.
        /// (UnitPrice * Quantity)
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
