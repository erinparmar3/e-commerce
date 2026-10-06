using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        builder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
        });

        // PasswordResetToken
        builder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasOne(t => t.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(t => t.Token);
        });

        // SellerProfile
        builder.Entity<SellerProfile>(entity =>
        {
            entity.HasOne(s => s.User)
                .WithOne(u => u.SellerProfile)
                .HasForeignKey<SellerProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(s => s.ShopName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(20);
        });

        // Category
        builder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        // Product
        builder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.SellerId);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.Name);
        });

        // Cart
        builder.Entity<Cart>(entity =>
        {
            entity.HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        builder.Entity<CartItem>(entity =>
        {
            entity.HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        });

        // WishlistItem
        builder.Entity<WishlistItem>(entity =>
        {
            entity.HasOne(w => w.User)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(w => w.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
        });

        // Order
        builder.Entity<Order>(entity =>
        {
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.FullName).IsRequired().HasMaxLength(150);
            entity.Property(o => o.Phone).IsRequired().HasMaxLength(20);
            entity.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(300);
            entity.Property(o => o.City).IsRequired().HasMaxLength(100);
            entity.Property(o => o.State).IsRequired().HasMaxLength(100);
            entity.Property(o => o.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(o => o.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.UserId);
        });

        // OrderItem
        builder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.PriceAtPurchase).HasPrecision(18, 2);
            entity.Property(oi => oi.Subtotal).HasPrecision(18, 2);

            entity.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(oi => oi.Seller)
                .WithMany(s => s.OrderItems)
                .HasForeignKey(oi => oi.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(oi => oi.OrderId);
            entity.HasIndex(oi => oi.SellerId);
        });

        // Payment
        builder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.Status).IsRequired().HasMaxLength(20);

            entity.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Review
        builder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment).IsRequired().HasMaxLength(1000);

            entity.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.OrderItem)
                .WithMany(oi => oi.Reviews)
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(r => r.ProductId);
            entity.HasIndex(r => r.UserId);
        });

        // Notification
        builder.Entity<Notification>(entity =>
        {
            entity.Property(n => n.Message).IsRequired().HasMaxLength(500);

            entity.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(n => n.UserId);
        });
    }
}
