using System;

namespace FoodTruck.Web.Models
{
    /// View model for the Admin dashboard.
    /// Contains high-level statistics about the application
    /// that are useful for an administrator.
    public class Admin
    {
        /// Total number of orders in the system.
        public int TotalOrders { get; set; }

        /// Number of orders that are still pending (not yet accepted).
        public int PendingOrders { get; set; }

        /// Number of orders currently in progress in the kitchen.
        public int InProgressOrders { get; set; }

        /// Number of menu items currently defined.
        public int MenuItemCount { get; set; }

        /// Number of menu categories (e.g., Tacos, Drinks, Sides).
        public int CategoryCount { get; set; }

        /// Number of schedules configured (truck + location + time).
        public int ScheduleCount { get; set; }

        /// Number of registered users in the system.
        public int UserCount { get; set; }

        /// Last time the dashboard data was generated.
        public DateTime GeneratedAtUtc { get; set; }
    }
}