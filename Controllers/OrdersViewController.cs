using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("orders")]
public class OrdersViewController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersViewController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Info"] = "Please sign in to view your orders.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/orders" });
        }

        int userId = User.GetUserId();
        var orders = await _orderService.GetBuyerOrdersAsync(userId);
        return View("~/Views/Orders/Index.cshtml", orders);
    }

    [HttpGet("details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            int userId = User.GetUserId();
            var order = await _orderService.GetBuyerOrderByIdAsync(userId, id);
            return View("~/Views/Orders/Details.cshtml", order);
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("cancel/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            int userId = User.GetUserId();
            await _orderService.CancelOrderAsync(userId, id);
            TempData["Success"] = $"Order #{id} has been cancelled and any reserved stock released.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
