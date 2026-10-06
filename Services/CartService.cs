using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _db;
    private const decimal TaxRate = 0.10m; // 10% standard tax rate

    public CartService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CartResponseDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return MapCartResponse(cart);
    }

    public async Task<CartResponseDto> AddItemAsync(int userId, AddCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new BadRequestException("Product is unavailable or does not exist.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
        var targetQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

        if (targetQuantity > product.StockQuantity)
        {
            throw new BadRequestException($"Insufficient stock for '{product.Name}'. Available stock: {product.StockQuantity}.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = targetQuantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            cart.CartItems.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Reload to get fresh product navigations
        return await GetCartAsync(userId);
    }

    public async Task<CartResponseDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        if (dto.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be greater than zero.");
        }

        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

        if (item == null)
        {
            throw new NotFoundException("Cart item not found.");
        }

        var product = await _db.Products.FindAsync(item.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new BadRequestException("Product is no longer active or available.");
        }

        if (dto.Quantity > product.StockQuantity)
        {
            throw new BadRequestException($"Requested quantity exceeds available stock of {product.StockQuantity}.");
        }

        item.Quantity = dto.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<CartResponseDto> RemoveItemAsync(int userId, int cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);

        if (item == null)
        {
            throw new NotFoundException("Cart item not found.");
        }

        cart.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.CartItems.Clear();
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        return cart;
    }

    private static CartResponseDto MapCartResponse(Cart cart)
    {
        var items = cart.CartItems
            .Where(ci => ci.Product != null)
            .Select(ci =>
            {
                var price = ci.Product!.Price;
                var subtotal = price * ci.Quantity;
                return new CartItemResponseDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    Price = price,
                    Quantity = ci.Quantity,
                    Subtotal = subtotal,
                    ImageUrl = ci.Product.ImageUrl,
                    StockQuantity = ci.Product.StockQuantity,
                    IsActive = ci.Product.IsActive
                };
            })
            .ToList();

        var subtotalSum = items.Sum(i => i.Subtotal);
        var tax = Math.Round(subtotalSum * TaxRate, 2);
        var total = subtotalSum + tax;

        return new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items,
            Subtotal = subtotalSum,
            Tax = tax,
            Total = total
        };
    }
}
