using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Authorize]
[Route("notifications")]
public class NotificationsViewController : Controller
{
    private readonly INotificationService _notificationService;

    public NotificationsViewController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        int userId = User.GetUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);
        return View("~/Views/Notifications/Index.cshtml", notifications);
    }

    [HttpPost("read/{id:int}")]
    public async Task<IActionResult> MarkRead(int id)
    {
        int userId = User.GetUserId();
        await _notificationService.MarkAsReadAsync(userId, id);
        return RedirectToAction(nameof(Index));
    }
}
