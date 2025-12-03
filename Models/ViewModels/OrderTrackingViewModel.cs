using FoodTruck.Web.Models.Enums;
using System;
using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{

    /// View model used for order confirmation and real-time tracking screens.
    public class OrderTrackingViewModel
    {
    
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? PickupEta { get; set; }
        public string? CancelReason { get; set; }
        public decimal? OrderTotal { get; set; }
        public bool CanCustomerCancel { get; set; }
        public List<string> ItemSummaries { get; set; } = new();
    
        /// Optional: When the order last updated its status.
        /// Helps track if status changed recently.
        public DateTime? LastUpdated { get; set; }
    
        /// Returns the ETA formatted nicely for UI.
        /// Example: "Today at 5:35 PM" or "Tomorrow at 11:00 AM".
        public string FormattedEta
        {
            get
            {
                if (PickupEta == null)
                    return "—";

                var eta = PickupEta.Value.ToLocalTime();
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                if (eta.Date == today)
                {
                    return $"Today at {eta:hh:mm tt}";
                }
                if (eta.Date == tomorrow)
                {
                    return $"Tomorrow at {eta:hh:mm tt}";
                }

                return eta.ToString("MMM dd 'at' hh:mm tt");
            }
        }
    }
}
