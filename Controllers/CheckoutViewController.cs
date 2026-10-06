using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("checkout")]
public class CheckoutViewController : Controller
{
    private readonly ICartService _cartService;
    private readonly ICheckoutService _checkoutService;
    private readonly IOrderService _orderService;
    private readonly IAuthService _authService;

    public CheckoutViewController(
        ICartService cartService,
        ICheckoutService checkoutService,
        IOrderService orderService,
        IAuthService authService)
    {
        _cartService = cartService;
        _checkoutService = checkoutService;
        _orderService = orderService;
        _authService = authService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Info"] = "Please sign in to proceed with checkout.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/checkout" });
        }

        int userId = User.GetUserId();
        var cart = await _cartService.GetCartAsync(userId);

        if (!cart.Items.Any())
        {
            TempData["Info"] = "Your cart is empty. Add items before checking out.";
            return Redirect("/cart");
        }

        var profile = await _authService.GetProfileAsync(userId);
        var model = new CheckoutRequestDto
        {
            FullName = profile.Name,
            Phone = profile.Phone ?? "",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            ShippingAddress = "247 Tech Boulevard, 4th Block, Koramangala",
            PaymentMethod = "CARD"
        };

        ViewBag.Cart = cart;
        return View("~/Views/Checkout/Index.cshtml", model);
    }

    [HttpPost("")]
    [HttpPost("process")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process([FromForm] CheckoutRequestDto dto)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = User.GetUserId();

        if (!ModelState.IsValid)
        {
            var cart = await _cartService.GetCartAsync(userId);
            ViewBag.Cart = cart;
            return View("~/Views/Checkout/Index.cshtml", dto);
        }

        try
        {
            var result = await _checkoutService.CheckoutAsync(userId, dto);
            TempData["Success"] = "Your order was successfully placed!";
            return RedirectToAction(nameof(Confirmation), new { id = result.OrderId });
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return Redirect("/cart");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Checkout failed: {ex.Message}";
            return Redirect("/cart");
        }
    }

    [HttpGet("confirmation/{id:int}")]
    public async Task<IActionResult> Confirmation(int id)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            int userId = User.GetUserId();
            var order = await _orderService.GetBuyerOrderByIdAsync(userId, id);
            return View("~/Views/Checkout/Confirmation.cshtml", order);
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }
}
