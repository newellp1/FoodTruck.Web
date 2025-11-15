using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    public class Modifier
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// e.g., Size, Protein, Add-on.
        /// </summary>
        [Required, StringLength(50)]
        public string Type { get; set; } = string.Empty;

        public decimal PriceDelta { get; set; }

        public ICollection<MenuItemModifier> MenuItemModifiers { get; set; } = new List<MenuItemModifier>();
    }
}