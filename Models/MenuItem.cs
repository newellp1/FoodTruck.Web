using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 9999)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        public int MenuCategoryId { get; set; }
        public MenuCategory MenuCategory { get; set; } = default!;

        public ICollection<MenuItemModifier> MenuItemModifiers { get; set; } = new List<MenuItemModifier>();
    }
}