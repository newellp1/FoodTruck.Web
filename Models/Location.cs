using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Address { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}