using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    // Represents a scheduled appearance of a food truck at a specific location and time.
    // Contains the truck, location, start and end times, and whether the schedule is currently active.
    public class Schedule
    {
        public int Id { get; set; }

        [Required]
        public int TruckId { get; set; }
        public Truck Truck { get; set; } = default!;

        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; } = default!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

    
        /// Indicates which schedule is currently active for a truck.
    
        public bool IsActive { get; set; }
    }
}