using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTruck.Web.Models
{
    /// A physical location where the food truck can appear.
    public class Location
    {
        /// Primary key.
        public int Id { get; set; }

        /// Name of the location (e.g., "Downtown Lot", "Campus Plaza").
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// Street address (line 1), used by existing views.
        /// Example: "123 Main St".
        [Display(Name = "Street Address")]
        [StringLength(200)]
        public string? Address { get; set; }

        /// City part of the address (optional).
        [StringLength(100)]
        public string? City { get; set; }

        /// State / province code (e.g., "TN").
        [StringLength(2)]
        public string? State { get; set; }

        /// Postal / ZIP code.
        [Display(Name = "Zip Code")]
        [StringLength(10)]
        public string? ZipCode { get; set; }

        /// Latitude of the location.
        [Display(Name = "Latitude")]
        public double? Latitude { get; set; }

        /// Longitude of the location.
        [Display(Name = "Longitude")]
        public double? Longitude { get; set; }

        /// Extra notes shown only in admin (e.g., parking instructions).
        [StringLength(500)]
        public string? Notes { get; set; }

        /// Whether this location is currently active and should appear in drop-downs.
        public bool IsActive { get; set; } = true;

        /// Navigation: schedules that use this location.
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}