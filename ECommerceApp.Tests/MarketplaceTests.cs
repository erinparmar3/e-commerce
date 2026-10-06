using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Models;
using ECommerceApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace ECommerceApp.Tests;

public class MarketplaceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AuthService_RegisterAndLogin_Success()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var hasher = new PasswordHasher();
        var jwtSettings = Options.Create(new JwtSettings());
        var tokenGen = new JwtTokenGenerator(jwtSettings);
        var authService = new AuthService(db, hasher, tokenGen, NullLogger<AuthService>.Instance);

        // Act - Register
        var registerDto = new RegisterDto
        {
            Name = "John Buyer",
            Email = "john@example.com",
            Password = "SecurePassword123!",
            Phone = "+91 99999 88888"
        };
        var registered = await authService.RegisterAsync(registerDto);

        // Assert - Register
        Assert.NotNull(registered);
        Assert.Equal("john@example.com", registered.Email);
        Assert.Equal(UserRoles.Buyer, registered.Role);

        // Act - Login
        var loginResponse = await authService.LoginAsync(new LoginDto
        {
            Email = "john@example.com",
            Password = "SecurePassword123!"
        });

        // Assert - Login
        Assert.NotNull(loginResponse);
        Assert.NotEmpty(loginResponse.Token);
        Assert.Equal("john@example.com", loginResponse.User.Email);
    }

    [Fact]
    public async Task AuthService_RegisterDuplicateEmail_ThrowsConflictException()
    {
        using var db = CreateInMemoryDbContext();
        var hasher = new PasswordHasher();
        var jwtSettings = Options.Create(new JwtSettings());
        var tokenGen = new JwtTokenGenerator(jwtSettings);
        var authService = new AuthService(db, hasher, tokenGen, NullLogger<AuthService>.Instance);

        var dto = new RegisterDto
        {
            Name = "Alice",
            Email = "alice@example.com",
            Password = "Password123!"
        };

        await authService.RegisterAsync(dto);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => authService.RegisterAsync(dto));
    }

    [Fact]
    public async Task SellerService_ApplyAndAdminApprove_UpdatesRole()
    {
        using var db = CreateInMemoryDbContext();
        var notifService = new NotificationService(db);
        var sellerService = new SellerService(db, notifService);

        var user = new User
        {
            Name = "Merchant Bob",
            Email = "bob@example.com",
            PasswordHash = "hash",
            Role = UserRoles.Buyer
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Apply as seller
        var applied = await sellerService.ApplyAsync(user.Id, new SellerApplicationDto
        {
            ShopName = "Bob's Electronics",
            Description = "Quality electronics hardware vendor."
        });

        Assert.Equal(SellerStatuses.Pending, applied.Status);

        // Admin approves
        var approved = await sellerService.ApproveSellerAsync(applied.Id);

        Assert.Equal(SellerStatuses.Approved, approved.Status);
        var updatedUser = await db.Users.FindAsync(user.Id);
        Assert.Equal(UserRoles.Seller, updatedUser!.Role);
    }

    [Fact]
    public async Task ProductService_SellerCannotModifyAnotherSellersProduct()
    {
        using var db = CreateInMemoryDbContext();
        var productService = new ProductService(db);

        // Seller A & Seller B
        var userA = new User { Name = "Seller A", Email = "a@test.com", Role = UserRoles.Seller };
        var userB = new User { Name = "Seller B", Email = "b@test.com", Role = UserRoles.Seller };
        db.Users.AddRange(userA, userB);
        await db.SaveChangesAsync();

        var profileA = new SellerProfile { UserId = userA.Id, ShopName = "Shop A", Status = SellerStatuses.Approved };
        var profileB = new SellerProfile { UserId = userB.Id, ShopName = "Shop B", Status = SellerStatuses.Approved };
        db.SellerProfiles.AddRange(profileA, profileB);

        var category = new Category { Name = "Gadgets", IsActive = true };
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var productA = new Product
        {
            SellerId = profileA.Id,
            CategoryId = category.Id,
            Name = "Product A",
            Description = "Description A",
            Price = 100m,
            StockQuantity = 10,
            IsActive = true
        };
        db.Products.Add(productA);
        await db.SaveChangesAsync();

        // Seller B attempts to update Product A
        var updateDto = new UpdateProductDto
        {
            CategoryId = category.Id,
            Name = "Hacked Name",
            Description = "Hacked Description",
            Price = 50m,
            StockQuantity = 5
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            productService.UpdateSellerProductAsync(userB.Id, productA.Id, updateDto));
    }

    [Fact]
    public async Task CartService_AddProductAndCalculateTotals()
    {
        using var db = CreateInMemoryDbContext();
        var cartService = new CartService(db);

        var user = new User { Name = "Buyer Charlie", Email = "charlie@test.com" };
        var seller = new SellerProfile { ShopName = "Shop", Status = SellerStatuses.Approved };
        var category = new Category { Name = "Books" };
        db.Users.Add(user);
        db.SellerProfiles.Add(seller);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Name = "Algorithms Book",
            Description = "Textbook",
            Price = 500m,
            StockQuantity = 10,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Add 2 units to cart
        var cart = await cartService.AddItemAsync(user.Id, new AddCartItemDto
        {
            ProductId = product.Id,
            Quantity = 2
        });

        Assert.Single(cart.Items);
        Assert.Equal(1000m, cart.Subtotal);
        Assert.Equal(100m, cart.Tax); // 10%
        Assert.Equal(1100m, cart.Total);
    }

    [Fact]
    public async Task CartService_InsufficientStock_ThrowsBadRequestException()
    {
        using var db = CreateInMemoryDbContext();
        var cartService = new CartService(db);

        var user = new User { Name = "Buyer Dave", Email = "dave@test.com" };
        var seller = new SellerProfile { ShopName = "Shop", Status = SellerStatuses.Approved };
        var category = new Category { Name = "Books" };
        db.Users.Add(user);
        db.SellerProfiles.Add(seller);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Name = "Limited Edition Item",
            Description = "Rare",
            Price = 1000m,
            StockQuantity = 3, // only 3 available
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Try to add 5 items
        await Assert.ThrowsAsync<BadRequestException>(() =>
            cartService.AddItemAsync(user.Id, new AddCartItemDto
            {
                ProductId = product.Id,
                Quantity = 5
            }));
    }

    [Fact]
    public async Task CheckoutService_SuccessfulCheckout_ReducesInventoryAndClearsCart()
    {
        using var db = CreateInMemoryDbContext();
        var notifService = new NotificationService(db);
        var cartService = new CartService(db);
        var checkoutService = new CheckoutService(db, notifService);

        var user = new User { Name = "Buyer Eve", Email = "eve@test.com" };
        var seller = new SellerProfile { ShopName = "Apex Store", Status = SellerStatuses.Approved };
        var category = new Category { Name = "Hardware" };
        db.Users.Add(user);
        db.SellerProfiles.Add(seller);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Name = "SSD Drive 1TB",
            Description = "NVMe SSD",
            Price = 5000m,
            StockQuantity = 20,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Add 2 items to cart
        await cartService.AddItemAsync(user.Id, new AddCartItemDto { ProductId = product.Id, Quantity = 2 });

        // Checkout
        var checkoutDto = new CheckoutRequestDto
        {
            FullName = "Eve Johnson",
            Phone = "+91 99887 76655",
            ShippingAddress = "123 Main Street",
            City = "Mumbai",
            State = "Maharashtra",
            PostalCode = "400001",
            PaymentMethod = "CARD"
        };

        var result = await checkoutService.CheckoutAsync(user.Id, checkoutDto);

        // Assert
        Assert.Equal(OrderStatuses.Confirmed, result.OrderStatus);
        Assert.Equal(PaymentStatuses.Success, result.PaymentStatus);

        // Check inventory reduced: 20 - 2 = 18
        var updatedProduct = await db.Products.FindAsync(product.Id);
        Assert.Equal(18, updatedProduct!.StockQuantity);

        // Check cart cleared
        var clearedCart = await cartService.GetCartAsync(user.Id);
        Assert.Empty(clearedCart.Items);
    }

    [Fact]
    public async Task ReviewService_NonPurchaserCannotReviewProduct()
    {
        using var db = CreateInMemoryDbContext();
        var reviewService = new ReviewService(db);

        var user = new User { Name = "Frank", Email = "frank@test.com" };
        var seller = new SellerProfile { ShopName = "Store", Status = SellerStatuses.Approved };
        var category = new Category { Name = "Toys" };
        db.Users.Add(user);
        db.SellerProfiles.Add(seller);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var product = new Product
        {
            SellerId = seller.Id,
            CategoryId = category.Id,
            Name = "Action Figure",
            Description = "Toy",
            Price = 500m,
            StockQuantity = 10,
            IsActive = true
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Frank attempts to review without purchasing
        var reviewDto = new CreateReviewDto
        {
            Rating = 5,
            Comment = "Looks great in photos!"
        };

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            reviewService.CreateReviewAsync(user.Id, product.Id, reviewDto));
    }

    [Fact]
    public async Task OrderService_InvalidStatusTransition_ThrowsBadRequestException()
    {
        using var db = CreateInMemoryDbContext();
        var notifService = new NotificationService(db);
        var orderService = new OrderService(db, notifService);

        var user = new User { Name = "Grace", Email = "grace@test.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var order = new Order
        {
            UserId = user.Id,
            TotalAmount = 1000m,
            FullName = "Grace",
            Phone = "+91 90000 00000",
            ShippingAddress = "Street",
            City = "Delhi",
            State = "Delhi",
            PostalCode = "110001",
            Status = OrderStatuses.Delivered // already delivered
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Attempting to cancel an already delivered order
        await Assert.ThrowsAsync<BadRequestException>(() =>
            orderService.UpdateAdminOrderStatusAsync(order.Id, OrderStatuses.Cancelled));
    }
}
