using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceApp.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 999999)]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = "/images/placeholder.png";

    [StringLength(60)]
    public string Category { get; set; } = "General";

    // Real stock tracking so "add to cart" can't oversell.
    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}
