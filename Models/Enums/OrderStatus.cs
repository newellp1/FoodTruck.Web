namespace FoodTruck.Web.Models.Enums
{
    /// Enumeration representing the various statuses an order can have in the system.
    public enum OrderStatus
    {
        Pending = 0,
        Accepted = 1,
        Preparing = 2,
        Ready = 3,
        Completed = 4,
        Cancelled = 5,
        Rejected = 6


    }
}