using System.ComponentModel.DataAnnotations;
using FoodTruck.Web.Models.Enums;

namespace FoodTruck.Web.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? CustomerId { get; set; }
        public ApplicationUser? Customer { get; set; }

        [Required, StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PickupEta { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal Total { get; set; }

        [StringLength(250)]
        public string? CancelReason { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}