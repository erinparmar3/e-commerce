using ECommerceApp.Models;

namespace ECommerceApp.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        bool hasProducts;
        try
        {
            hasProducts = context.Products.Any();
        }
        catch
        {
            // If the Products table doesn't exist yet, try to ensure database is created
            context.Database.EnsureCreated();
            hasProducts = context.Products.Any();
        }

        if (hasProducts)
        {
            return; // already seeded
        }

        var products = new List<Product>
        {
            new() { Name = "Wireless Headphones", Description = "Over-ear Bluetooth headphones with noise cancellation.", Price = 79.99m, Category = "Electronics", StockQuantity = 50, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard with blue switches.", Price = 59.50m, Category = "Electronics", StockQuantity = 35, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Running Shoes", Description = "Lightweight breathable running shoes.", Price = 45.00m, Category = "Footwear", StockQuantity = 80, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Ceramic Coffee Mug", Description = "350ml ceramic mug, dishwasher safe.", Price = 9.99m, Category = "Home", StockQuantity = 200, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Backpack", Description = "Water-resistant 25L daily backpack.", Price = 34.99m, Category = "Bags", StockQuantity = 60, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Smart Watch", Description = "Fitness tracking smart watch with heart-rate monitor.", Price = 99.00m, Category = "Electronics", StockQuantity = 40, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Desk Lamp", Description = "Adjustable LED desk lamp with USB charging port.", Price = 24.99m, Category = "Home", StockQuantity = 70, ImageUrl = "/images/placeholder.png" },
            new() { Name = "Yoga Mat", Description = "Non-slip 6mm yoga mat.", Price = 19.99m, Category = "Fitness", StockQuantity = 90, ImageUrl = "/images/placeholder.png" },
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
