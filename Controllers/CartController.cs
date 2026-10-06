using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> GetCart()
    {
        var userId = User.GetUserId();
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart));
    }

    [HttpPost("items")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> AddItem([FromBody] AddCartItemDto dto)
    {
        var userId = User.GetUserId();
        var cart = await _cartService.AddItemAsync(userId, dto);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Item added to cart."));
    }

    [HttpPut("items/{id}")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> UpdateItem(
        [FromRoute] int id,
        [FromBody] UpdateCartItemDto dto)
    {
        var userId = User.GetUserId();
        var cart = await _cartService.UpdateItemAsync(userId, id, dto);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Cart item quantity updated."));
    }

    [HttpDelete("items/{id}")]
    public async Task<ActionResult<ApiResponse<CartResponseDto>>> RemoveItem([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var cart = await _cartService.RemoveItemAsync(userId, id);
        return Ok(ApiResponse<CartResponseDto>.Ok(cart, "Item removed from cart."));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>> ClearCart()
    {
        var userId = User.GetUserId();
        var result = await _cartService.ClearCartAsync(userId);
        return Ok(ApiResponse<bool>.Ok(result, "Cart cleared successfully."));
    }
}
