using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }
        public List<int> ModifierIds { get; set; } = new();
        public string? ModifierSummary { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}