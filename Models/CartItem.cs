namespace ECommerceApp.Models;

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal LineTotal => Quantity * (Product?.Price ?? 0);
}
