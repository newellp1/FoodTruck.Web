using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    /// Represents a tag that can be applied to menu items (e.g., "Spicy", "Vegetarian", "Gluten-Free").
    /// Enables filtering and categorization of menu items by dietary preferences and attributes.
    public class Tag
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string BadgeColor { get; set; } = "secondary"; // Bootstrap badge color: primary, success, danger, warning, info, etc.

        public int DisplayOrder { get; set; } = 0;

        // Navigation property for many-to-many relationship
        public ICollection<MenuItemTag> MenuItemTags { get; set; } = new List<MenuItemTag>();
    }
}
