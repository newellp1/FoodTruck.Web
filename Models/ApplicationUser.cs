using Microsoft.AspNetCore.Identity;

namespace FoodTruck.Web.Models
{
    /// <summary>
    /// Custom application user for ASP.NET Identity.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // Optional: additional profile fields
        public string? DisplayName { get; set; }
        public string? DefaultPhone { get; set; }
        public string? AllergyNotes { get; set; }
    }
}