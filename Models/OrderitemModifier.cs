namespace FoodTruck.Web.Models
{
    public class OrderItemModifier
    {
        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = default!;

        public int ModifierId { get; set; }
        public Modifier Modifier { get; set; } = default!;
    }
}