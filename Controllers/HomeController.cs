using System.Security.Claims;
using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models;
using ECommerceManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home Storefront Catalog
        public async Task<IActionResult> Index(
            string? searchString,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortOrder,
            int pageNumber = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            // Category Filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Price Filters
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sorting
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "name_asc" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.Id) // Default: Newest
            };

            int pageSize = 8;
            var paginatedList = await PaginatedList<Product>.CreateAsync(query, pageNumber, pageSize);

            var viewModel = new ProductFilterViewModel
            {
                Products = paginatedList,
                Categories = await _context.Categories.ToListAsync(),
                SearchString = searchString,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                PageNumber = pageNumber
            };

            ViewBag.FeaturedProducts = await _context.Products
                .Where(p => p.IsFeatured)
                .Take(4)
                .ToListAsync();

            return View(viewModel);
        }

        // Product Details & Customer Reviews View
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Post Customer Product Review
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your product review has been submitted.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }
    }
}
