namespace FoodTruck.Web.Models
{
    /// Join table entity for the many-to-many relationship between MenuItems and Tags.
    /// Allows a menu item to have multiple tags and a tag to be applied to multiple menu items.
    public class MenuItemTag
    {
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
