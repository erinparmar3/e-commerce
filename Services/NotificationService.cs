using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;

    public NotificationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(n => new NotificationResponseDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new NotFoundException("Notification not found.");
        }

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task CreateNotificationAsync(int userId, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }
}
