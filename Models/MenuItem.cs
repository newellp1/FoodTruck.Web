using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        
        [Display(Name = "MenuCategory")]
        [Required(ErrorMessage = "Please choose a category.")]
        public int MenuCategoryId { get; set; }

        // Navigation only – NO [Required] here
        public MenuCategory? MenuCategory { get; set; }

        // Optional extras we added earlier
        public string? ImagePath { get; set; }
        public int DisplayOrder { get; set; } = 0;

        // Navigation property for many-to-many relationship with Tags
        public ICollection<MenuItemTag> MenuItemTags { get; set; } = new List<MenuItemTag>();

    }
}