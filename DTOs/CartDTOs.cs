using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class AddCartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}

public class UpdateCartItemDto
{
    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}

public class CartItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class CartResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemResponseDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
}
