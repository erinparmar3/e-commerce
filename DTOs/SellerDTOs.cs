using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class SellerApplicationDto
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string ShopName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateSellerProfileDto
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string ShopName { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;
}

public class SellerProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
}
