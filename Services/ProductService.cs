using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _db;

    public ProductService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PaginatedResult<ProductResponseDto>> GetProductsAsync(ProductQueryParameters query)
    {
        var baseQuery = _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            baseQuery = baseQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            baseQuery = baseQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        var totalCount = await baseQuery.CountAsync();

        var sortedQuery = query.Sort?.ToLower() switch
        {
            "price_asc" => baseQuery.OrderBy(p => p.Price),
            "price_desc" => baseQuery.OrderByDescending(p => p.Price),
            "name_asc" => baseQuery.OrderBy(p => p.Name),
            "name_desc" => baseQuery.OrderByDescending(p => p.Name),
            "newest" => baseQuery.OrderByDescending(p => p.CreatedAt),
            _ => baseQuery.OrderByDescending(p => p.CreatedAt)
        };

        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var items = await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id,
                SellerId = p.SellerId,
                SellerShopName = p.Seller != null ? p.Seller.ShopName : string.Empty,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : string.Empty,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                AverageRating = p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0.0,
                ReviewCount = p.Reviews.Count,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return new PaginatedResult<ProductResponseDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductResponseDto> GetProductByIdAsync(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {id} was not found or is inactive.");
        }

        return MapToDto(product);
    }

    public async Task<ProductResponseDto> CreateSellerProductAsync(int userId, CreateProductDto dto)
    {
        var sellerProfile = await _db.SellerProfiles
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (sellerProfile == null || sellerProfile.Status != SellerStatuses.Approved)
        {
            throw new ForbiddenException("Only approved sellers can create products.");
        }

        var category = await _db.Categories.FindAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new BadRequestException("The specified category does not exist or is inactive.");
        }

        if (dto.Price <= 0)
        {
            throw new BadRequestException("Product price must be greater than zero.");
        }

        if (dto.StockQuantity < 0)
        {
            throw new BadRequestException("Stock quantity cannot be negative.");
        }

        var product = new Product
        {
            SellerId = sellerProfile.Id,
            CategoryId = dto.CategoryId,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        // Reload with navigations
        await _db.Entry(product).Reference(p => p.Seller).LoadAsync();
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();

        return MapToDto(product);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetSellerProductsAsync(int userId)
    {
        var sellerProfile = await _db.SellerProfiles.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId);
        if (sellerProfile == null)
        {
            throw new ForbiddenException("Seller profile not found.");
        }

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.SellerId == sellerProfile.Id)
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductResponseDto> GetSellerProductByIdAsync(int userId, int productId)
    {
        var sellerProfile = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sellerProfile == null)
        {
            throw new ForbiddenException("Seller profile not found.");
        }

        var product = await _db.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        if (product.SellerId != sellerProfile.Id)
        {
            throw new ForbiddenException("You are not authorized to view another seller's private product details.");
        }

        return MapToDto(product);
    }

    public async Task<ProductResponseDto> UpdateSellerProductAsync(int userId, int productId, UpdateProductDto dto)
    {
        var sellerProfile = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sellerProfile == null || sellerProfile.Status != SellerStatuses.Approved)
        {
            throw new ForbiddenException("Only approved sellers can update products.");
        }

        var product = await _db.Products
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        if (product.SellerId != sellerProfile.Id)
        {
            throw new ForbiddenException("You cannot modify another seller's product.");
        }

        var category = await _db.Categories.FindAsync(dto.CategoryId);
        if (category == null || !category.IsActive)
        {
            throw new BadRequestException("The specified category does not exist or is inactive.");
        }

        if (dto.Price <= 0)
        {
            throw new BadRequestException("Product price must be greater than zero.");
        }

        if (dto.StockQuantity < 0)
        {
            throw new BadRequestException("Stock quantity cannot be negative.");
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.CategoryId = dto.CategoryId;
        product.ImageUrl = dto.ImageUrl?.Trim();
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<bool> DeleteSellerProductAsync(int userId, int productId)
    {
        var sellerProfile = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (sellerProfile == null || sellerProfile.Status != SellerStatuses.Approved)
        {
            throw new ForbiddenException("Only approved sellers can delete products.");
        }

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        if (product.SellerId != sellerProfile.Id)
        {
            throw new ForbiddenException("You cannot delete another seller's product.");
        }

        // Rule: Prefer deactivating products instead of physically deleting if they appear in orders
        var hasOrders = await _db.OrderItems.AnyAsync(oi => oi.ProductId == productId);
        if (hasOrders)
        {
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Remove from cart and wishlists first, then delete
            var cartItems = await _db.CartItems.Where(ci => ci.ProductId == productId).ToListAsync();
            _db.CartItems.RemoveRange(cartItems);

            var wishlistItems = await _db.WishlistItems.Where(w => w.ProductId == productId).ToListAsync();
            _db.WishlistItems.RemoveRange(wishlistItems);

            _db.Products.Remove(product);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    private static ProductResponseDto MapToDto(Product p)
    {
        return new ProductResponseDto
        {
            Id = p.Id,
            SellerId = p.SellerId,
            SellerShopName = p.Seller?.ShopName ?? string.Empty,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            ImageUrl = p.ImageUrl,
            IsActive = p.IsActive,
            AverageRating = p.Reviews != null && p.Reviews.Any() ? Math.Round(p.Reviews.Average(r => r.Rating), 1) : 0.0,
            ReviewCount = p.Reviews?.Count ?? 0,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
