using ECommerceManagementSystem.Data;
using ECommerceManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC Services
builder.Services.AddControllersWithViews();

// 2. Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Add ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 4. Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// 5. Add Session for Shopping Cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 6. Configure Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. Seed Database (Roles, Admin User, Categories & Products)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations automatically
        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Admin Account
        string adminEmail = "admin@ecommerce.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                City = "Tech City",
                Address = "100 Admin HQ Way",
                PostalCode = "90001",
                CreatedAt = DateTime.UtcNow
            };

            var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed Categories
        if (!await context.Categories.AnyAsync())
        {
            var electronics = new Category { Name = "Electronics" };
            var fashion = new Category { Name = "Fashion & Apparel" };
            var home = new Category { Name = "Home & Kitchen" };
            var books = new Category { Name = "Books & Stationery" };

            context.Categories.AddRange(electronics, fashion, home, books);
            await context.SaveChangesAsync();
        }

        // Seed Rich Sample Products if Products table has fewer than 5 items
        if (await context.Products.CountAsync() < 5)
        {
            var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
            var fashion = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Fashion & Apparel");
            var home = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Home & Kitchen");
            var books = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Books & Stationery");

            var defaultCategory = electronics ?? await context.Categories.FirstAsync();

            var sampleProducts = new List<Product>
            {
                new Product
                {
                    Name = "Enterprise Flagship Smartphone 5G",
                    Description = "Next-generation 6.7-inch OLED 120Hz display, 5G connectivity, 256GB storage, pro camera system.",
                    Price = 999.99m,
                    Stock = 25,
                    CategoryId = electronics?.Id ?? defaultCategory.Id,
                    IsFeatured = true,
                    ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Pro Noise-Canceling Wireless Headphones",
                    Description = "Immersive spatial audio technology with active noise cancellation and 30-hour battery life.",
                    Price = 249.99m,
                    Stock = 40,
                    CategoryId = electronics?.Id ?? defaultCategory.Id,
                    IsFeatured = true,
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Ultra-Thin Developer Laptop 16GB",
                    Description = "High-performance M-series processor, 16GB RAM, 512GB NVMe SSD, crisp Retina display.",
                    Price = 1299.00m,
                    Stock = 12,
                    CategoryId = electronics?.Id ?? defaultCategory.Id,
                    IsFeatured = true,
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Classic Genuine Leather Oxford Shoes",
                    Description = "Handcrafted premium genuine leather footwear designed for formal elegance and all-day comfort.",
                    Price = 139.50m,
                    Stock = 18,
                    CategoryId = fashion?.Id ?? defaultCategory.Id,
                    IsFeatured = false,
                    ImageUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Designer Waterproof Chronograph Watch",
                    Description = "Stainless steel casing, scratch-resistant sapphire crystal glass, 50m water resistance.",
                    Price = 299.00m,
                    Stock = 15,
                    CategoryId = fashion?.Id ?? defaultCategory.Id,
                    IsFeatured = true,
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Smart Italian Pump Espresso Machine",
                    Description = "Programmable 15-bar pressure Italian espresso and cappuccino maker with milk frother steam wand.",
                    Price = 189.00m,
                    Stock = 10,
                    CategoryId = home?.Id ?? defaultCategory.Id,
                    IsFeatured = true,
                    ImageUrl = "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "Ergonomic Mesh Executive Office Chair",
                    Description = "Adjustable lumbar support, 3D armrests, breathable mesh back, heavy-duty aluminum base.",
                    Price = 219.99m,
                    Stock = 8,
                    CategoryId = home?.Id ?? defaultCategory.Id,
                    IsFeatured = false,
                    ImageUrl = "https://images.unsplash.com/photo-1580481072645-022f9a6d83d0?w=600&auto=format&fit=crop"
                },
                new Product
                {
                    Name = "The Clean Architecture Guide Book",
                    Description = "Essential software engineering principles for building robust, maintainable .NET applications.",
                    Price = 45.00m,
                    Stock = 50,
                    CategoryId = books?.Id ?? defaultCategory.Id,
                    IsFeatured = false,
                    ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&auto=format&fit=crop"
                }
            };

            context.Products.AddRange(sampleProducts);
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
