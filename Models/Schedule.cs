using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
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