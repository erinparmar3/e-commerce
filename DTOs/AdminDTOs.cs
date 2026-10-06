using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalSellers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
}

public class AdminUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? SellerShopName { get; set; }
    public string? SellerStatus { get; set; }
}

public class UpdateUserStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}
