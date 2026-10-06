using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("wishlist")]
public class WishlistViewController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistViewController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet("")]
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Info"] = "Please sign in to view your saved wishlist.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/wishlist" });
        }

        int userId = User.GetUserId();
        var items = await _wishlistService.GetWishlistAsync(userId);
        return View("~/Views/Wishlist/Index.cshtml", items);
    }

    [HttpPost("toggle/{productId:int}")]
    [HttpGet("toggle/{productId:int}")]
    public async Task<IActionResult> ToggleRoute(int productId)
    {
        return await HandleToggleAsync(productId);
    }

    [HttpPost("toggle")]
    [HttpGet("toggle")]
    public async Task<IActionResult> ToggleQuery([FromQuery(Name = "productId")] int? queryId, [FromForm(Name = "productId")] int? formId)
    {
        int productId = queryId ?? formId ?? 0;
        return await HandleToggleAsync(productId);
    }

    [HttpPost("remove/{productId:int}")]
    [HttpGet("remove/{productId:int}")]
    public async Task<IActionResult> RemoveRoute(int productId)
    {
        return await HandleRemoveAsync(productId);
    }

    [HttpPost("remove")]
    [HttpGet("remove")]
    public async Task<IActionResult> RemoveQuery([FromQuery(Name = "productId")] int? queryId, [FromForm(Name = "productId")] int? formId)
    {
        int productId = queryId ?? formId ?? 0;
        return await HandleRemoveAsync(productId);
    }

    private async Task<IActionResult> HandleToggleAsync(int productId)
    {
        if (productId <= 0)
        {
            TempData["Error"] = "Invalid product.";
            return RedirectToAction(nameof(Index));
        }

        bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                      Request.Headers.Accept.ToString().Contains("application/json");

        if (!User.Identity?.IsAuthenticated == true)
        {
            string returnUrl = GetSafeReturnUrl(productId);
            if (isAjax)
            {
                return StatusCode(401, new { success = false, message = "Please sign in to save items to your wishlist.", redirectUrl = $"/account/login?returnUrl={Uri.EscapeDataString(returnUrl)}" });
            }
            TempData["Info"] = "Please sign in to save items to your wishlist.";
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        bool currentlyInWishlist = false;
        try
        {
            int userId = User.GetUserId();
            var wishlist = await _wishlistService.GetWishlistAsync(userId);
            bool alreadyInWishlist = wishlist.Any(w => w.ProductId == productId);

            if (alreadyInWishlist)
            {
                await _wishlistService.RemoveFromWishlistAsync(userId, productId);
                currentlyInWishlist = false;
                TempData["Info"] = "Item removed from your wishlist.";
            }
            else
            {
                await _wishlistService.AddToWishlistAsync(userId, productId);
                currentlyInWishlist = true;
                TempData["Success"] = "Item saved to your wishlist!";
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
                return StatusCode(500, new { success = false, message = "Failed to update wishlist." });
            }
            TempData["Error"] = "Failed to update wishlist.";
        }

        if (isAjax)
        {
            return Json(new { success = true, inWishlist = currentlyInWishlist, message = currentlyInWishlist ? "Saved to wishlist!" : "Removed from wishlist." });
        }

        return RedirectToRefererOrWishlist();
    }

    private async Task<IActionResult> HandleRemoveAsync(int productId)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (productId > 0)
        {
            try
            {
                int userId = User.GetUserId();
                await _wishlistService.RemoveFromWishlistAsync(userId, productId);
                TempData["Success"] = "Item removed from wishlist.";
            }
            catch (AppException ex)
            {
                TempData["Error"] = ex.Message;
            }
        }

        return RedirectToRefererOrWishlist();
    }

    private IActionResult RedirectToRefererOrWishlist()
    {
        string? referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            try
            {
                var uri = new Uri(referer);
                string pathAndQuery = uri.PathAndQuery;
                if (!string.IsNullOrEmpty(pathAndQuery) && 
                    !pathAndQuery.Contains("/wishlist/toggle", StringComparison.OrdinalIgnoreCase) &&
                    !pathAndQuery.Contains("/wishlist/remove", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(pathAndQuery);
                }
            }
            catch
            {
                if (Url.IsLocalUrl(referer) && 
                    !referer.Contains("/wishlist/toggle", StringComparison.OrdinalIgnoreCase) &&
                    !referer.Contains("/wishlist/remove", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(referer);
                }
            }
        }

        return RedirectToAction(nameof(Index));
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
                    !pathAndQuery.Contains("/wishlist/toggle", StringComparison.OrdinalIgnoreCase) &&
                    !pathAndQuery.Contains("/account/login", StringComparison.OrdinalIgnoreCase))
                {
                    return pathAndQuery;
                }
            }
            catch { }
        }
        return $"/product/{productId}";
    }
}
