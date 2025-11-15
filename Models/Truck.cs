using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    public class Truck
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}