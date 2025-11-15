namespace FoodTruck.Web.Models
{
    public class MenuItemModifier
    {
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = default!;

        public int ModifierId { get; set; }
        public Modifier Modifier { get; set; } = default!;

        public int? MinSelections { get; set; }
        public int? MaxSelections { get; set; }
    }
}