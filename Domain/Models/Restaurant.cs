using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Restaurant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(320)]
        public string OwnerEmailAddress { get; set; } = string.Empty;

        // e.g. "Pending", "Approved"
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }
    }
}
