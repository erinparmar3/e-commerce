using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public CategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool activeOnly = true)
    {
        var query = _db.Categories.AsNoTracking().AsQueryable();
        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        var categories = await query
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories;
    }

    public async Task<CategoryResponseDto> GetCategoryByIdAsync(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                ProductCount = c.Products.Count(p => p.IsActive)
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            throw new NotFoundException($"Category with ID {id} not found.");
        }

        return category;
    }

    public async Task<PaginatedResult<ProductResponseDto>> GetCategoryProductsAsync(int categoryId, ProductQueryParameters query)
    {
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {categoryId} not found.");
        }

        var productsQuery = _db.Products
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Seller)
            .Include(p => p.Category)
            .Include(p => p.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        productsQuery = query.Sort?.ToLower() switch
        {
            "price_asc" => productsQuery.OrderBy(p => p.Price),
            "price_desc" => productsQuery.OrderByDescending(p => p.Price),
            "name_asc" => productsQuery.OrderBy(p => p.Name),
            "name_desc" => productsQuery.OrderByDescending(p => p.Name),
            "newest" => productsQuery.OrderByDescending(p => p.CreatedAt),
            _ => productsQuery.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await productsQuery.CountAsync();
        var page = query.Page > 0 ? query.Page : 1;
        var pageSize = query.PageSize > 0 ? query.PageSize : 20;

        var items = await productsQuery
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

    public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var normalizedName = dto.Name.Trim();
        var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == normalizedName.ToLower());
        if (exists)
        {
            throw new ConflictException($"A category named '{normalizedName}' already exists.");
        }

        var category = new Category
        {
            Name = normalizedName,
            Description = dto.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            ProductCount = 0
        };
    }

    public async Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {id} not found.");
        }

        var normalizedName = dto.Name.Trim();
        var duplicate = await _db.Categories.AnyAsync(c => c.Id != id && c.Name.ToLower() == normalizedName.ToLower());
        if (duplicate)
        {
            throw new ConflictException($"Another category with name '{normalizedName}' already exists.");
        }

        category.Name = normalizedName;
        category.Description = dto.Description?.Trim();
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var count = await _db.Products.CountAsync(p => p.CategoryId == id && p.IsActive);

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            ProductCount = count
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null)
        {
            throw new NotFoundException($"Category with ID {id} not found.");
        }

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            // Soft-deactivate if products are attached
            category.IsActive = false;
            category.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Categories.Remove(category);
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
