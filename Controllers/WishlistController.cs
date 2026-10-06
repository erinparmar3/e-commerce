using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WishlistItemResponseDto>>>> GetWishlist()
    {
        var userId = User.GetUserId();
        var items = await _wishlistService.GetWishlistAsync(userId);
        return Ok(ApiResponse<IEnumerable<WishlistItemResponseDto>>.Ok(items));
    }

    [HttpPost("{productId}")]
    public async Task<ActionResult<ApiResponse<bool>>> AddToWishlist([FromRoute] int productId)
    {
        var userId = User.GetUserId();
        var result = await _wishlistService.AddToWishlistAsync(userId, productId);
        return Ok(ApiResponse<bool>.Ok(result, "Product added to wishlist."));
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromWishlist([FromRoute] int productId)
    {
        var userId = User.GetUserId();
        var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);
        return Ok(ApiResponse<bool>.Ok(result, "Product removed from wishlist."));
    }
}
