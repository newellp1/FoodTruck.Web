using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    // Represents an item within a customer's order.
    /// Contains details about the menu item ordered, quantity, price, and any special notes.
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;

        [Required]
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = default!;

        [Range(1, 100)]
        public int Quantity { get; set; }

        [Range(0.01, 9999)]
        public decimal UnitPrice { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }
   }
}