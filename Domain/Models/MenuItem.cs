using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Domain.Interfaces;

namespace Domain.Models
{
    public class MenuItem : IItemValidating
    {
        // EF primary key (NOT populated from JSON)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public int Id { get; set; }

        // JSON "id"
        [JsonPropertyName("id")]
        public string ExternalId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [MaxLength(3)]
        [JsonPropertyName("currency")]
        public string? Currency { get; set; } = "EUR";

        // DB foreign key
        [JsonIgnore]
        [Required]
        public int RestaurantId { get; set; }

        [ForeignKey(nameof(RestaurantId))]
        public Restaurant? Restaurant { get; set; }

        // JSON "restaurantId"
        [JsonPropertyName("restaurantId")]
        public string RestaurantExternalId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? ImageFileName { get; set; }

        public List<string> GetValidators()
        {
            var emails = new List<string>();

            // Restaurant owner can approve menu items
            if (Restaurant != null && !string.IsNullOrWhiteSpace(Restaurant.OwnerEmailAddress))
            {
                emails.Add(Restaurant.OwnerEmailAddress);
            }

            return emails;
        }

        public string GetCardPartial()
        {
            return "_MenuItemRow";
        }
    }
}
