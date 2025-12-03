using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{

    /// ViewModel used by MenuController.Index and Views/Menu/Index.cshtml.
    /// Carries the active schedule, next schedule, and the menu categories
    /// (including sub-categories and items) to the view.

    public class MenuViewModel
    {
    
        /// The schedule that is currently active (truck + location + times),
        /// or null if the truck is currently closed.
    
        public Schedule? ActiveSchedule { get; set; }

    
        /// The next upcoming schedule after now, shown if there is no
        /// currently active schedule (e.g., "We're closed, next: Thu 11:00 AM").
    
        public Schedule? NextSchedule { get; set; }

    
        /// Menu categories to render on the page.
        /// These are usually top-level categories (ParentCategoryId == null),
        /// each of which may have:
        ///   - MenuItems directly
        ///   - SubCategories with their own MenuItems
    
        public IEnumerable<MenuCategory> Categories { get; set; } = new List<MenuCategory>();

    
        /// Current search term entered by the user (if any).
        /// Used to display it back in the search box.
    
        public string? SearchTerm { get; set; }

    
        /// IDs of selected categories used for filtering.
        /// Can include both parent and sub-category IDs.
    
        public IEnumerable<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}