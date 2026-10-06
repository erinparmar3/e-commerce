using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CheckoutResponseDto>>> Checkout([FromBody] CheckoutRequestDto dto)
    {
        var userId = User.GetUserId();
        var result = await _checkoutService.CheckoutAsync(userId, dto);
        return Ok(ApiResponse<CheckoutResponseDto>.Ok(result, result.Message));
    }
}
