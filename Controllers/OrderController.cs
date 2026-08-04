using System.Security.Claims;
using System.Text.Json;
using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models;
using ECommerceManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartSessionKey = "CartSessionKey";

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private List<CartItem> GetCartFromSession()
        {
            var sessionData = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(sessionData)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(sessionData) ?? new List<CartItem>();
        }

        // GET: Checkout
        public async Task<IActionResult> Checkout()
        {
            var cartItems = GetCartFromSession();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add products before checking out.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var vm = new CheckoutViewModel
            {
                CustomerName = user?.FullName ?? "",
                ShippingAddress = user?.Address ?? "",
                City = user?.City ?? "",
                PostalCode = user?.PostalCode ?? "",
                Cart = new CartViewModel { Items = cartItems }
            };

            return View(vm);
        }

        // POST: Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cartItems = GetCartFromSession();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    CustomerName = model.CustomerName,
                    ShippingAddress = model.ShippingAddress,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    PaymentMethod = model.PaymentMethod,
                    Status = OrderStatus.Pending,
                    TotalAmount = cartItems.Sum(i => i.SubTotal)
                };

                foreach (var item in cartItems)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });

                    // Update stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock = Math.Max(0, product.Stock - item.Quantity);
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Clear Cart Session
                HttpContext.Session.Remove(CartSessionKey);

                return RedirectToAction(nameof(Confirmation), new { id = order.Id });
            }

            model.Cart = new CartViewModel { Items = cartItems };
            return View(model);
        }

        // GET: Confirmation
        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Customer Order History
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Order Details
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Check authorization: User can only see own orders unless Admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(order);
        }

        // Printable Invoice View
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: Admin Order Management
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrders(OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            ViewBag.CurrentStatus = status;
            return View(orders);
        }

        // POST: Update Order Status (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Order #{orderId} status updated to {status}.";
            }
            return RedirectToAction(nameof(AdminOrders));
        }
    }
}
