using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerceManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, int? categoryId, int pageNumber = 1)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString) || p.Description.Contains(searchString));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.SearchString = searchString;

            int pageSize = 10;
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery.OrderByDescending(p => p.Id), pageNumber, pageSize);
            return View(paginatedProducts);
        }

        // GET: Product/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? primaryImage, List<IFormFile>? galleryImages)
        {
            if (ModelState.IsValid)
            {
                // Primary Image Upload
                if (primaryImage != null && primaryImage.Length > 0)
                {
                    product.ImageUrl = await UploadFileAsync(primaryImage);
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Multi-Image Gallery Uploads
                if (galleryImages != null && galleryImages.Count > 0)
                {
                    foreach (var image in galleryImages)
                    {
                        if (image.Length > 0)
                        {
                            string imgPath = await UploadFileAsync(image);
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imgPath
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? primaryImage, List<IFormFile>? galleryImages)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null) return NotFound();

                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.IsFeatured = product.IsFeatured;

                if (primaryImage != null && primaryImage.Length > 0)
                {
                    existingProduct.ImageUrl = await UploadFileAsync(primaryImage);
                }

                _context.Update(existingProduct);
                await _context.SaveChangesAsync();

                if (galleryImages != null && galleryImages.Count > 0)
                {
                    foreach (var image in galleryImages)
                    {
                        if (image.Length > 0)
                        {
                            string imgPath = await UploadFileAsync(image);
                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = imgPath
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var img = await _context.ProductImages.FindAsync(imageId);
            if (img != null)
            {
                int productId = img.ProductId;
                _context.ProductImages.Remove(img);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gallery image removed.";
                return RedirectToAction(nameof(Edit), new { id = productId });
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> UploadFileAsync(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }
    }
}