using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalProducts = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            var totalUsers = await _context.Users.CountAsync();

            var recentProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Chart data: Product count per Category
            var categoryStats = await _context.Categories
                .Select(c => new
                {
                    Name = c.Name,
                    ProductCount = c.Products.Count
                })
                .ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                RecentProducts = recentProducts,
                RecentOrders = recentOrders,
                CategoryNames = categoryStats.Select(cs => cs.Name).ToList(),
                CategoryProductCounts = categoryStats.Select(cs => cs.ProductCount).ToList()
            };

            return View(vm);
        }
    }
}
