using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models.ViewModels
{
    // ViewModel for looking up an order by various criteria.
    public class OrderLookupViewModel
    {
        [Display(Name = "Order #")]
        public int? OrderId { get; set; }

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone Number")]
        [Phone]
        public string? Phone { get; set; }
        
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}