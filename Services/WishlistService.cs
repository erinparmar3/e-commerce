using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _db;

    public WishlistService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<WishlistItemResponseDto>> GetWishlistAsync(int userId)
    {
        var items = await _db.WishlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return items.Select(w => new WishlistItemResponseDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product?.Name ?? string.Empty,
            Price = w.Product?.Price ?? 0m,
            ImageUrl = w.Product?.ImageUrl,
            IsActive = w.Product?.IsActive ?? false,
            StockQuantity = w.Product?.StockQuantity ?? 0,
            AddedAt = w.CreatedAt
        });
    }

    public async Task<bool> AddToWishlistAsync(int userId, int productId)
    {
        var product = await _db.Products.FindAsync(productId);
        if (product == null || !product.IsActive)
        {
            throw new NotFoundException("Product not found or is inactive.");
        }

        var exists = await _db.WishlistItems
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

        if (exists)
        {
            throw new ConflictException("Product is already in your wishlist.");
        }

        var item = new WishlistItem
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        };

        _db.WishlistItems.Add(item);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        var item = await _db.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (item == null)
        {
            throw new NotFoundException("Product not found in your wishlist.");
        }

        _db.WishlistItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}
