using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(1, 1000000, ErrorMessage = "Price must be between 1 and 1,000,000.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        // Navigation Property
        public Category? Category { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, 10000, ErrorMessage = "Stock must be between 0 and 10,000.")]
        public int Stock { get; set; }

        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Collections
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}