using Microsoft.AspNetCore.Mvc;

namespace ECommerceManagementSystem.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}