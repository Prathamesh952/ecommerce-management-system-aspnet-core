using System.ComponentModel.DataAnnotations;

namespace ECommerceManagementSystem.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Customer Name is required.")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping Address is required.")]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code is required.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment Method is required.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Credit Card";

        public CartViewModel Cart { get; set; } = new CartViewModel();
    }
}
