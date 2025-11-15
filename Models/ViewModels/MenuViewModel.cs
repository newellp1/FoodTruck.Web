using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{
    public class MenuViewModel
    {
        public Schedule? ActiveSchedule { get; set; }
        public Schedule? NextSchedule { get; set; }
        public IEnumerable<MenuCategory> Categories { get; set; } = new List<MenuCategory>();
        public string? SearchTerm { get; set; }
        public IEnumerable<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}