using System.ComponentModel.DataAnnotations;

namespace ECommerceManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50)]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, 10000)]
        public int Stock { get; set; }
    }
}