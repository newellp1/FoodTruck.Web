using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models.ViewModels
{
    // ViewModel for checkout process, including contact and payment info.
    public class CheckoutViewModel
    {
        // ============================
        // Contact info
        // ============================

        [Required, StringLength(100)]
        [Display(Name = "Full name")]
        public string ContactName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Display(Name = "Phone number")]
        public string ContactPhone { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Email address")]
        public string ContactEmail { get; set; } = string.Empty;
        
        // ============================
        // Payment info (fake / demo only)
        // ============================

        /// "Cash" or "Card". Default is Cash.
        [Required]
        [Display(Name = "Payment method")]
        public string PaymentMethod { get; set; } = "Cash";

        /// 12-digit fake card number, only required when PaymentMethod == "Card".
        /// Not stored in the database.
        [Display(Name = "Card number")]
        [StringLength(12, MinimumLength = 12,
            ErrorMessage = "Card number must be exactly 12 digits.")]
        [RegularExpression(@"^\d{12}$",
            ErrorMessage = "Card number must be exactly 12 digits (numbers only).")]
        public string? CardNumber { get; set; }

        /// Expiration in MM/YY format, only for demo.
        [Display(Name = "Expiry (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$",
            ErrorMessage = "Use MM/YY format, e.g. 04/27.")]
        public string? Expiry { get; set; }

        /// 3–4 digit fake CVV, only for demo.
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string? Cvv { get; set; }
    }
}