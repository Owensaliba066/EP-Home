using System.Collections.Generic;
using Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // Foreign key to Restaurant
        [Required]
        public int RestaurantId { get; set; }

        [ForeignKey(nameof(RestaurantId))]
        public Restaurant? Restaurant { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(3)]
        public string? Currency { get; set; } = "EUR";

        [MaxLength(500)]
        public string? ImageFileName { get; set; }

        public List<string> GetValidators()
        {
            var emails = new List<string>();

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
