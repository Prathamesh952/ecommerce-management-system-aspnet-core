namespace ECommerceManagementSystem.Models.ViewModels
{
    public class MonthlySalesData
    {
        public string Month { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopProductData
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ReportViewModel
    {
        public List<MonthlySalesData> MonthlySales { get; set; } = new List<MonthlySalesData>();
        public List<TopProductData> TopProducts { get; set; } = new List<TopProductData>();
        public decimal OverallRevenue { get; set; }
        public int OverallOrders { get; set; }
    }
}
