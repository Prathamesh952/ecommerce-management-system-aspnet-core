using Microsoft.EntityFrameworkCore;
using ECommerceManagementSystem.Models;

namespace ECommerceManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Products table
        public DbSet<Product> Products { get; set; }
    }
}