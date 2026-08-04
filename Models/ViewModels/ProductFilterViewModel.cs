namespace ECommerceManagementSystem.Models.ViewModels
{
    public class ProductFilterViewModel
    {
        public PaginatedList<Product>? Products { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();

        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
