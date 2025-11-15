using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required, StringLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string ContactPhone { get; set; } = string.Empty;
    }
}