namespace ECommerceManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }

        public List<Product> RecentProducts { get; set; } = new List<Product>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();

        // Chart Data
        public List<string> CategoryNames { get; set; } = new List<string>();
        public List<int> CategoryProductCounts { get; set; } = new List<int>();
    }
}
