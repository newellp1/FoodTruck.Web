using FoodTruck.Web.Models.Enums;

namespace FoodTruck.Web.Models.ViewModels
{
    public class OrderTrackingViewModel
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? PickupEta { get; set; }
        public string? CancelReason { get; set; }
    }
}