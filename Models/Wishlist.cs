using System.ComponentModel.DataAnnotations;

namespace ECommerceManagementSystem.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Required]
        public int ProductId { get; set; }

        public Product? Product { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
