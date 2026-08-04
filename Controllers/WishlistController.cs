using System.Security.Claims;
using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var wishlists = await _context.Wishlists
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Category)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlists);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (existing != null)
            {
                _context.Wishlists.Remove(existing);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item removed from your wishlist.";
            }
            else
            {
                _context.Wishlists.Add(new Wishlist
                {
                    UserId = userId,
                    ProductId = productId,
                    AddedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item added to your wishlist!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item removed from wishlist.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
