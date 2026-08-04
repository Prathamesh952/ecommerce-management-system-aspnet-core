namespace ECommerceManagementSystem.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal GrandTotal => Items.Sum(x => x.SubTotal);
    }
}
