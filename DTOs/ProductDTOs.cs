using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ECommerceApp.DTOs;

public class CreateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100000000.0, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }
}

public class UpdateProductDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 100000000.0, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, 1000000, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProductResponseDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public string SellerShopName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductQueryParameters
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    // Sort options: price_asc, price_desc, name_asc, name_desc, newest
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
