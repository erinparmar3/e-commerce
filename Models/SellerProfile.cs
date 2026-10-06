namespace ECommerceApp.Models;

public class SellerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = SellerStatuses.Pending; // Pending, Approved, Rejected, Suspended
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
