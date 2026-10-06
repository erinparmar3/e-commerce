using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _db;

    public AdminService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        var totalUsers = await _db.Users.CountAsync();
        var totalSellers = await _db.SellerProfiles.CountAsync(s => s.Status == SellerStatuses.Approved);
        var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
        var totalOrders = await _db.Orders.CountAsync();
        var totalSales = await _db.Orders
            .Where(o => o.Status != OrderStatuses.Cancelled)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalSellers = totalSellers,
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            TotalSales = totalSales
        };
    }

    public async Task<IEnumerable<AdminUserDto>> GetUsersAsync()
    {
        var users = await _db.Users
            .Include(u => u.SellerProfile)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            SellerShopName = u.SellerProfile?.ShopName,
            SellerStatus = u.SellerProfile?.Status
        });
    }

    public async Task<AdminUserDto> UpdateUserStatusAsync(int userId, bool isActive)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new AdminUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SellerShopName = user.SellerProfile?.ShopName,
            SellerStatus = user.SellerProfile?.Status
        };
    }
}
