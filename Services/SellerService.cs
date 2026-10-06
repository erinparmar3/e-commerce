using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class SellerService : ISellerService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public SellerService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<SellerProfileDto> ApplyAsync(int userId, SellerApplicationDto dto)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.SellerProfile != null)
        {
            if (user.SellerProfile.Status == SellerStatuses.Approved)
            {
                throw new BadRequestException("You are already an approved seller.");
            }
            if (user.SellerProfile.Status == SellerStatuses.Pending)
            {
                throw new BadRequestException("Your seller application is already pending review.");
            }

            // If previously rejected, allow re-application
            user.SellerProfile.ShopName = dto.ShopName.Trim();
            user.SellerProfile.Description = dto.Description.Trim();
            user.SellerProfile.Status = SellerStatuses.Pending;
            user.SellerProfile.CreatedAt = DateTime.UtcNow;
            user.SellerProfile.ApprovedAt = null;
        }
        else
        {
            var profile = new SellerProfile
            {
                UserId = userId,
                ShopName = dto.ShopName.Trim(),
                Description = dto.Description.Trim(),
                Status = SellerStatuses.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.SellerProfiles.Add(profile);
        }

        await _db.SaveChangesAsync();

        return await GetProfileAsync(userId);
    }

    public async Task<SellerProfileDto> GetProfileAsync(int userId)
    {
        var profile = await _db.SellerProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException("Seller profile not found for this user.");
        }

        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ShopName = profile.ShopName,
            Description = profile.Description,
            Status = profile.Status,
            CreatedAt = profile.CreatedAt,
            ApprovedAt = profile.ApprovedAt,
            OwnerName = profile.User?.Name ?? string.Empty,
            OwnerEmail = profile.User?.Email ?? string.Empty
        };
    }

    public async Task<SellerProfileDto> UpdateProfileAsync(int userId, UpdateSellerProfileDto dto)
    {
        var profile = await _db.SellerProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (profile == null)
        {
            throw new NotFoundException("Seller profile not found.");
        }

        profile.ShopName = dto.ShopName.Trim();
        profile.Description = dto.Description.Trim();

        await _db.SaveChangesAsync();

        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ShopName = profile.ShopName,
            Description = profile.Description,
            Status = profile.Status,
            CreatedAt = profile.CreatedAt,
            ApprovedAt = profile.ApprovedAt,
            OwnerName = profile.User?.Name ?? string.Empty,
            OwnerEmail = profile.User?.Email ?? string.Empty
        };
    }

    public async Task<IEnumerable<SellerProfileDto>> GetPendingSellersAsync()
    {
        var pending = await _db.SellerProfiles
            .Include(s => s.User)
            .Where(s => s.Status == SellerStatuses.Pending)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync();

        return pending.Select(p => new SellerProfileDto
        {
            Id = p.Id,
            UserId = p.UserId,
            ShopName = p.ShopName,
            Description = p.Description,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            ApprovedAt = p.ApprovedAt,
            OwnerName = p.User?.Name ?? string.Empty,
            OwnerEmail = p.User?.Email ?? string.Empty
        });
    }

    public async Task<SellerProfileDto> ApproveSellerAsync(int sellerProfileId)
    {
        var profile = await _db.SellerProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sellerProfileId);

        if (profile == null)
        {
            throw new NotFoundException("Seller profile not found.");
        }

        profile.Status = SellerStatuses.Approved;
        profile.ApprovedAt = DateTime.UtcNow;

        if (profile.User != null)
        {
            // Give user seller role while retaining purchasing ability
            if (profile.User.Role != UserRoles.Admin)
            {
                profile.User.Role = UserRoles.Seller;
            }
            await _notificationService.CreateNotificationAsync(
                profile.UserId,
                $"Congratulations! Your seller application for '{profile.ShopName}' has been approved. You can now list and manage products.");
        }

        await _db.SaveChangesAsync();

        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ShopName = profile.ShopName,
            Description = profile.Description,
            Status = profile.Status,
            CreatedAt = profile.CreatedAt,
            ApprovedAt = profile.ApprovedAt,
            OwnerName = profile.User?.Name ?? string.Empty,
            OwnerEmail = profile.User?.Email ?? string.Empty
        };
    }

    public async Task<SellerProfileDto> RejectSellerAsync(int sellerProfileId)
    {
        var profile = await _db.SellerProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == sellerProfileId);

        if (profile == null)
        {
            throw new NotFoundException("Seller profile not found.");
        }

        profile.Status = SellerStatuses.Rejected;

        if (profile.User != null)
        {
            if (profile.User.Role == UserRoles.Seller)
            {
                profile.User.Role = UserRoles.Buyer;
            }
            await _notificationService.CreateNotificationAsync(
                profile.UserId,
                $"Your seller application for '{profile.ShopName}' has been reviewed and rejected.");
        }

        await _db.SaveChangesAsync();

        return new SellerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ShopName = profile.ShopName,
            Description = profile.Description,
            Status = profile.Status,
            CreatedAt = profile.CreatedAt,
            ApprovedAt = profile.ApprovedAt,
            OwnerName = profile.User?.Name ?? string.Empty,
            OwnerEmail = profile.User?.Email ?? string.Empty
        };
    }
}
