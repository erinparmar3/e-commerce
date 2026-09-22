namespace ECommerceApp.Models;

public class Cart
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public List<CartItem> CartItems { get; set; } = new();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal Total => CartItems.Sum(ci => ci.Quantity * (ci.Product?.Price ?? 0));

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int ItemCount => CartItems.Sum(ci => ci.Quantity);
}
