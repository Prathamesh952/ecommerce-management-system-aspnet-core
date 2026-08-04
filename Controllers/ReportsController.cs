using System.Text;
using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var monthlySales = orders
                .GroupBy(o => o.OrderDate.ToString("yyyy-MM"))
                .Select(g => new MonthlySalesData
                {
                    Month = g.Key,
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(m => m.Month)
                .ToList();

            var topProducts = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product?.Name ?? "Unknown Product")
                .Select(g => new TopProductData
                {
                    ProductName = g.Key,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(p => p.TotalQuantitySold)
                .Take(10)
                .ToList();

            var vm = new ReportViewModel
            {
                MonthlySales = monthlySales,
                TopProducts = topProducts,
                OverallRevenue = orders.Sum(o => o.TotalAmount),
                OverallOrders = orders.Count
            };

            return View(vm);
        }

        public async Task<IActionResult> ExportCsv()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("OrderId,CustomerName,Email,OrderDate,Status,TotalAmount,PaymentMethod");

            foreach (var order in orders)
            {
                builder.AppendLine($"{order.Id},\"{order.CustomerName}\",\"{order.User?.Email}\",{order.OrderDate:yyyy-MM-dd HH:mm},{order.Status},{order.TotalAmount},\"{order.PaymentMethod}\"");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Sales_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}
