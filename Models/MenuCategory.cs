using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    /// Represents a menu category, e.g., "Beverages", "Soda", "Tacos".
    /// Supports sub-categories via ParentCategoryId.
    public class MenuCategory
    {
        /// Primary key.
        public int Id { get; set; }

        /// Display name of the category (e.g., "Beverages", "Soda").
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// Optional description shown in admin screens.
        [StringLength(255)]
        public string? Description { get; set; }

        /// Sort categories in a specific order on the menu.
        /// Lower numbers appear first.
        public int DisplayOrder { get; set; }

        // ============================
        // Sub-category support
        // ============================

        /// Optional foreign key pointing to this category's parent category.
        /// If null, this category is a top-level category (e.g., "Beverages").
        /// If set, this is a sub-category (e.g., "Soda" under "Beverages").
        public int? ParentCategoryId { get; set; }

        /// Navigation property to the parent category.
        public MenuCategory? ParentCategory { get; set; }

        /// Navigation property for child categories (sub-categories).
        /// For example, "Beverages" can have sub-categories "Soda" and "Coffee".
        public ICollection<MenuCategory> SubCategories { get; set; } = new List<MenuCategory>();

        /// Menu items directly under this category (or sub-category).
        /// For example, "Coke" and "Pepsi" might belong to the "Soda" category.
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}