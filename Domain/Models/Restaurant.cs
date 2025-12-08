using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace Domain.Models
{
    public class Restaurant : IItemValidating
    {
        // EF primary key (NOT populated from JSON)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        // JSON "id" -> e.g. "R-1001"
        [JsonPropertyName("id")]
        public string ExternalId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [JsonPropertyName("ownerEmailAddress")]
        [Required]
        [EmailAddress]
        public string OwnerEmailAddress { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        [MaxLength(50)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageFileName { get; set; }

        // Needed for filtering Approved/Pending and for the views
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        // Who can approve a restaurant? The site admin.
        public List<string> GetValidators()
        {
            return new List<string>
            {
                "siteadmin@example.com"
            };
        }

        // Which partial to use in the catalog / verification views
        public string GetCardPartial()
        {
            return "_RestaurantCard";
        }
    }
}
