namespace ECommerceApp.DTOs;

public class WishlistItemResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int StockQuantity { get; set; }
    public DateTime AddedAt { get; set; }
}
