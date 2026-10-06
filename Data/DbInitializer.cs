using ECommerceApp.Common;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data;

public static class DbInitializer
{
    public static void Seed(ApplicationDbContext db, IPasswordHasher hasher)
    {
        if (db.Users.Any())
        {
            return; // DB has already been seeded
        }

        // 1. Admin
        var admin = new User
        {
            Name = "System Administrator",
            Email = "admin@marketplace.com",
            PasswordHash = hasher.HashPassword("AdminPassword123!"),
            Phone = "+91 98765 43210",
            Role = UserRoles.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };
        db.Users.Add(admin);

        // 2. Approved Sellers
        var seller1User = new User
        {
            Name = "Tech Hub Electronics",
            Email = "seller1@techhub.com",
            PasswordHash = hasher.HashPassword("SellerPassword123!"),
            Phone = "+91 98111 22233",
            Role = UserRoles.Seller,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        var seller2User = new User
        {
            Name = "Urban Threads Store",
            Email = "seller2@fashiontrend.com",
            PasswordHash = hasher.HashPassword("SellerPassword123!"),
            Phone = "+91 98222 33344",
            Role = UserRoles.Seller,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        var seller3User = new User
        {
            Name = "KitchenPrime Supplies",
            Email = "seller3@homekitchen.com",
            PasswordHash = hasher.HashPassword("SellerPassword123!"),
            Phone = "+91 98333 44455",
            Role = UserRoles.Seller,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        db.Users.AddRange(seller1User, seller2User, seller3User);
        db.SaveChanges();

        var seller1Profile = new SellerProfile
        {
            UserId = seller1User.Id,
            ShopName = "TechHub Official",
            Description = "Top rated computer hardware, mobile accessories, and electronics vendor.",
            Status = SellerStatuses.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            ApprovedAt = DateTime.UtcNow.AddDays(-29)
        };

        var seller2Profile = new SellerProfile
        {
            UserId = seller2User.Id,
            ShopName = "Urban Threads Store",
            Description = "Modern fashion apparel, casual wear, and premium lifestyle accessories.",
            Status = SellerStatuses.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            ApprovedAt = DateTime.UtcNow.AddDays(-19)
        };

        var seller3Profile = new SellerProfile
        {
            UserId = seller3User.Id,
            ShopName = "KitchenPrime",
            Description = "High quality cookware, smart kitchen appliances, and culinary tools.",
            Status = SellerStatuses.Approved,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ApprovedAt = DateTime.UtcNow.AddDays(-9)
        };

        db.SellerProfiles.AddRange(seller1Profile, seller2Profile, seller3Profile);
        db.SaveChanges();

        // 3. Buyers
        var buyer1 = new User
        {
            Name = "Aarav Sharma",
            Email = "buyer1@gmail.com",
            PasswordHash = hasher.HashPassword("BuyerPassword123!"),
            Phone = "+91 91234 56780",
            Role = UserRoles.Buyer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        var buyer2 = new User
        {
            Name = "Priya Patel",
            Email = "buyer2@gmail.com",
            PasswordHash = hasher.HashPassword("BuyerPassword123!"),
            Phone = "+91 91234 56781",
            Role = UserRoles.Buyer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        var buyer3 = new User
        {
            Name = "Rohan Gupta",
            Email = "buyer3@gmail.com",
            PasswordHash = hasher.HashPassword("BuyerPassword123!"),
            Phone = "+91 91234 56782",
            Role = UserRoles.Buyer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        var buyer4 = new User
        {
            Name = "Ananya Iyer",
            Email = "buyer4@gmail.com",
            PasswordHash = hasher.HashPassword("BuyerPassword123!"),
            Phone = "+91 91234 56783",
            Role = UserRoles.Buyer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart()
        };

        db.Users.AddRange(buyer1, buyer2, buyer3, buyer4);
        db.SaveChanges();

        // 4. Categories
        var catElectronics = new Category { Name = "Electronics & Gadgets", Description = "Laptops, headphones, keyboards and smart gadgets", IsActive = true };
        var catFashion = new Category { Name = "Fashion & Apparel", Description = "Trendy clothing, footwear, and lifestyle accessories", IsActive = true };
        var catKitchen = new Category { Name = "Home & Kitchen", Description = "Cookware, appliances, and home decoration essentials", IsActive = true };
        var catBooks = new Category { Name = "Books & Stationery", Description = "Academic books, novels, diaries, and writing instruments", IsActive = true };
        var catFitness = new Category { Name = "Sports & Fitness", Description = "Fitness gear, gym accessories, and sports equipment", IsActive = true };

        db.Categories.AddRange(catElectronics, catFashion, catKitchen, catBooks, catFitness);
        db.SaveChanges();

        // 5. Products
        var products = new List<Product>
        {
            new()
            {
                SellerId = seller1Profile.Id,
                CategoryId = catElectronics.Id,
                Name = "ProBook Ultra 15 Laptop",
                Description = "High performance Intel Core i7 13th Gen laptop with 16GB RAM, 512GB NVMe SSD, and 15.6 inch FHD IPS display.",
                Price = 64999.00m,
                StockQuantity = 25,
                ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853",
                IsActive = true
            },
            new()
            {
                SellerId = seller1Profile.Id,
                CategoryId = catElectronics.Id,
                Name = "Wireless Noise Cancelling Headphones",
                Description = "Over-ear bluetooth 5.3 headphones with active noise cancellation, deep bass, and 40-hour battery life.",
                Price = 4999.00m,
                StockQuantity = 80,
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e",
                IsActive = true
            },
            new()
            {
                SellerId = seller1Profile.Id,
                CategoryId = catElectronics.Id,
                Name = "RGB Mechanical Gaming Keyboard",
                Description = "Tenkeyless mechanical gaming keyboard with tactile blue switches, per-key RGB backlighting, and braided cable.",
                Price = 2899.00m,
                StockQuantity = 45,
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3",
                IsActive = true
            },
            new()
            {
                SellerId = seller1Profile.Id,
                CategoryId = catElectronics.Id,
                Name = "Ergonomic Optical Gaming Mouse",
                Description = "Precision gaming mouse with 16000 DPI sensor, customizable side buttons, and lightweight honeycomb shell.",
                Price = 1499.00m,
                StockQuantity = 100,
                ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46",
                IsActive = true
            },
            new()
            {
                SellerId = seller2Profile.Id,
                CategoryId = catFashion.Id,
                Name = "Classic Slim Fit Cotton Denim Jacket",
                Description = "Premium washed denim jacket with buttoned chest pockets and timeless rugged styling for all seasons.",
                Price = 2499.00m,
                StockQuantity = 50,
                ImageUrl = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0",
                IsActive = true
            },
            new()
            {
                SellerId = seller2Profile.Id,
                CategoryId = catFashion.Id,
                Name = "Casual Breathable Canvas Sneakers",
                Description = "Lightweight canvas lace-up sneakers with anti-slip rubber outsole and cushioned insole.",
                Price = 1799.00m,
                StockQuantity = 60,
                ImageUrl = "https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77",
                IsActive = true
            },
            new()
            {
                SellerId = seller2Profile.Id,
                CategoryId = catFashion.Id,
                Name = "Genuine Leather Minimalist Wallet",
                Description = "Handcrafted genuine leather bifold wallet with RFID blocking technology and multiple card slots.",
                Price = 899.00m,
                StockQuantity = 120,
                ImageUrl = "https://images.unsplash.com/photo-1627123424574-724758594e93",
                IsActive = true
            },
            new()
            {
                SellerId = seller3Profile.Id,
                CategoryId = catKitchen.Id,
                Name = "Smart Touch Digital Air Fryer 5.5L",
                Description = "Oil-free rapid air circulation fryer with 8 preset cooking modes, non-stick basket, and digital LED display.",
                Price = 5499.00m,
                StockQuantity = 35,
                ImageUrl = "https://images.unsplash.com/photo-1556911220-e15b29be8c8f",
                IsActive = true
            },
            new()
            {
                SellerId = seller3Profile.Id,
                CategoryId = catKitchen.Id,
                Name = "Stainless Steel Chef Knife Set (5-Piece)",
                Description = "High carbon forged stainless steel kitchen knives with wooden block and ergonomic santoprene handles.",
                Price = 3299.00m,
                StockQuantity = 40,
                ImageUrl = "https://images.unsplash.com/photo-1593618998160-e34014e67546",
                IsActive = true
            },
            new()
            {
                SellerId = seller3Profile.Id,
                CategoryId = catKitchen.Id,
                Name = "Borosilicate Glass Food Storage Containers (Set of 4)",
                Description = "Airtight leakproof microwave and oven safe food containers with BPA-free locking lids.",
                Price = 1199.00m,
                StockQuantity = 90,
                ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae",
                IsActive = true
            }
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        // 6. Sample Initial Confirmed Order with Review for Buyer 1
        var sampleProduct1 = products[0]; // Laptop
        var sampleProduct2 = products[3]; // Mouse

        var sampleOrder = new Order
        {
            UserId = buyer1.Id,
            TotalAmount = sampleProduct1.Price + sampleProduct2.Price + ((sampleProduct1.Price + sampleProduct2.Price) * 0.10m),
            FullName = "Aarav Sharma",
            Phone = "+91 91234 56780",
            ShippingAddress = "Flat 402, Green Valley Apartments, MG Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Status = OrderStatuses.Delivered,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            OrderItems = new List<OrderItem>
            {
                new()
                {
                    ProductId = sampleProduct1.Id,
                    SellerId = sampleProduct1.SellerId,
                    ProductName = sampleProduct1.Name,
                    PriceAtPurchase = sampleProduct1.Price,
                    Quantity = 1,
                    Subtotal = sampleProduct1.Price
                },
                new()
                {
                    ProductId = sampleProduct2.Id,
                    SellerId = sampleProduct2.SellerId,
                    ProductName = sampleProduct2.Name,
                    PriceAtPurchase = sampleProduct2.Price,
                    Quantity = 1,
                    Subtotal = sampleProduct2.Price
                }
            }
        };

        sampleOrder.Payment = new Payment
        {
            OrderId = sampleOrder.Id,
            TransactionId = "TXN_INIT_9876543210AB",
            Amount = sampleOrder.TotalAmount,
            Status = PaymentStatuses.Success,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            PaidAt = DateTime.UtcNow.AddDays(-5)
        };

        db.Orders.Add(sampleOrder);
        db.SaveChanges();

        // 7. Sample Review from Buyer 1 for the Laptop
        var sampleReview = new Review
        {
            ProductId = sampleProduct1.Id,
            UserId = buyer1.Id,
            OrderItemId = sampleOrder.OrderItems.First().Id,
            Rating = 5,
            Comment = "Outstanding performance and battery life! The FHD IPS display is crystal clear.",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
        db.Reviews.Add(sampleReview);

        // 8. Sample Notifications
        db.Notifications.AddRange(
            new Notification
            {
                UserId = buyer1.Id,
                Message = $"Your order #{sampleOrder.Id} has been delivered. Enjoy your purchase!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Notification
            {
                UserId = seller1User.Id,
                Message = "Congratulations! Your seller application for 'TechHub Official' has been approved.",
                IsRead = true,
                CreatedAt = DateTime.UtcNow.AddDays(-29)
            }
        );

        db.SaveChanges();
    }
}
