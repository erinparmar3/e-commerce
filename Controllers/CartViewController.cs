using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("cart")]
public class CartViewController : Controller
{
    private readonly ICartService _cartService;

    public CartViewController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Info"] = "Please sign in to view your shopping cart.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/cart" });
        }

        int userId = User.GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return View("~/Views/Cart/Index.cshtml", cart);
    }

    [HttpGet("/cartview")]
    [HttpGet("/cartview/index")]
    public IActionResult CartViewRedirect() => Redirect("/cart");

    [HttpPost("add/{productId:int}")]
    [HttpGet("add/{productId:int}")]
    public async Task<IActionResult> AddRoute(int productId, [FromQuery] int quantity = 1)
    {
        return await HandleAddAsync(productId, quantity);
    }

    [HttpPost("add")]
    [HttpGet("add")]
    public async Task<IActionResult> AddQuery(
        [FromQuery(Name = "productId")] int? queryProductId,
        [FromForm(Name = "productId")] int? formProductId,
        [FromQuery(Name = "quantity")] int? queryQuantity,
        [FromForm(Name = "quantity")] int? formQuantity)
    {
        int effectiveProductId = queryProductId ?? formProductId ?? 0;
        int effectiveQuantity = formQuantity ?? queryQuantity ?? 1;
        return await HandleAddAsync(effectiveProductId, effectiveQuantity);
    }

    private async Task<IActionResult> HandleAddAsync(int productId, int quantity)
    {
        if (quantity <= 0) quantity = 1;

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                      Request.Headers.Accept.ToString().Contains("application/json");

        if (productId <= 0)
        {
            if (isAjax)
            {
                return BadRequest(new { success = false, message = "Invalid product." });
            }
            TempData["Error"] = "Invalid product.";
            return Redirect("/cart");
        }

        if (!User.Identity?.IsAuthenticated == true)
        {
            string returnUrl = GetSafeReturnUrl(productId);
            if (isAjax)
            {
                return StatusCode(401, new { success = false, message = "Please sign in to add items to your cart.", redirectUrl = $"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}" });
            }
            TempData["Info"] = "Please sign in to add items to your cart.";
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        try
        {
            int userId = User.GetUserId();
            var cart = await _cartService.AddItemAsync(userId, new AddCartItemDto
            {
                ProductId = productId,
                Quantity = quantity
            });

            TempData["Success"] = "Item added to your shopping cart!";
            if (isAjax)
            {
                int totalCount = cart.Items.Sum(i => i.Quantity);
                return Json(new { success = true, message = "Item added to your shopping cart!", cartCount = totalCount });
            }
        }
        catch (AppException ex)
        {
            if (isAjax)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            if (isAjax)
            {
                return StatusCode(500, new { success = false, message = "Could not add item to cart." });
            }
            TempData["Error"] = "Could not add item to cart.";
        }

        return Redirect("/cart");
    }

    [HttpPost("update")]
    [HttpGet("update")]
    public async Task<IActionResult> Update(
        [FromQuery(Name = "cartItemId")] int? queryItemId,
        [FromForm(Name = "cartItemId")] int? formItemId,
        [FromQuery(Name = "quantity")] int? queryQty,
        [FromForm(Name = "quantity")] int? formQty)
    {
        int effectiveItemId = queryItemId ?? formItemId ?? 0;
        int effectiveQty = formQty ?? queryQty ?? 1;

        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (effectiveItemId <= 0)
        {
            return Redirect("/cart");
        }

        try
        {
            int userId = User.GetUserId();
            if (effectiveQty <= 0)
            {
                await _cartService.RemoveItemAsync(userId, effectiveItemId);
                TempData["Success"] = "Item removed from cart.";
            }
            else
            {
                await _cartService.UpdateItemAsync(userId, effectiveItemId, new UpdateCartItemDto { Quantity = effectiveQty });
                TempData["Success"] = "Cart updated.";
            }
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not update cart.";
        }

        return Redirect("/cart");
    }

    [HttpPost("remove/{cartItemId:int}")]
    [HttpGet("remove/{cartItemId:int}")]
    public async Task<IActionResult> RemoveRoute(int cartItemId)
    {
        return await HandleRemoveAsync(cartItemId);
    }

    [HttpPost("remove")]
    [HttpGet("remove")]
    public async Task<IActionResult> RemoveQuery(
        [FromQuery(Name = "cartItemId")] int? queryItemId,
        [FromForm(Name = "cartItemId")] int? formItemId)
    {
        int effectiveItemId = queryItemId ?? formItemId ?? 0;
        return await HandleRemoveAsync(effectiveItemId);
    }

    private async Task<IActionResult> HandleRemoveAsync(int cartItemId)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (cartItemId > 0)
        {
            try
            {
                int userId = User.GetUserId();
                await _cartService.RemoveItemAsync(userId, cartItemId);
                TempData["Success"] = "Item removed from cart.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "Could not remove item from cart.";
            }
        }

        return Redirect("/cart");
    }

    [HttpPost("clear")]
    [HttpGet("clear")]
    public async Task<IActionResult> Clear()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            int userId = User.GetUserId();
            await _cartService.ClearCartAsync(userId);
            TempData["Success"] = "Your cart has been cleared.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not clear cart.";
        }

        return Redirect("/cart");
    }

    private string GetSafeReturnUrl(int productId)
    {
        string? referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            try
            {
                var uri = new Uri(referer);
                string pathAndQuery = uri.PathAndQuery;
                if (!string.IsNullOrEmpty(pathAndQuery) && 
                    !pathAndQuery.Contains("/cart", StringComparison.OrdinalIgnoreCase) &&
                    !pathAndQuery.Contains("/account/login", StringComparison.OrdinalIgnoreCase))
                {
                    return pathAndQuery;
                }
            }
            catch { }
        }
        return productId > 0 ? $"/product/{productId}" : "/cart";
    }
}
