using System.Collections.Generic;
using System.Linq;

namespace FoodTruck.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
    }
}