using System.Collections.Generic;

namespace FoodTruck.Web.Models.ViewModels
{
    /// Simple view model for listing users in the Admin Users page.
    /// Holds identity info + roles for display.
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? UserName { get; set; }

        /// All roles assigned to this user (e.g., Admin, Staff, Customer).
        public IList<string> Roles { get; set; } = new List<string>();
    }
}