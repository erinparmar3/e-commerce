using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationResponseDto>>>> GetNotifications()
    {
        var userId = User.GetUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return Ok(ApiResponse<IEnumerable<NotificationResponseDto>>.Ok(notifications));
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var result = await _notificationService.MarkAsReadAsync(userId, id);
        return Ok(ApiResponse<bool>.Ok(result, "Notification marked as read."));
    }
}
