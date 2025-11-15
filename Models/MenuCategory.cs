using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}