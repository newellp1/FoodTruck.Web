using System.Collections.Generic;
using System.Linq;

namespace FoodTruck.Web.Models.ViewModels
{
    // ViewModel representing a shopping cart with items and totals.
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        public decimal Subtotal => Items?.Sum(i => i.LineTotal) ?? 0m;
        public decimal Total => Items?.Sum(i => i.LineTotal) ?? 0m;
    }
}