namespace ECommerceApp.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int SellerId { get; set; }
    public SellerProfile? Seller { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal PriceAtPurchase { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
