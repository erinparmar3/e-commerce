using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SellersController : ControllerBase
{
    private readonly ISellerService _sellerService;

    public SellersController(ISellerService sellerService)
    {
        _sellerService = sellerService;
    }

    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<SellerProfileDto>>> Apply([FromBody] SellerApplicationDto dto)
    {
        var userId = User.GetUserId();
        var profile = await _sellerService.ApplyAsync(userId, dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SellerProfileDto>.Ok(profile, "Seller application submitted successfully and is pending admin review."));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<SellerProfileDto>>> GetProfile()
    {
        var userId = User.GetUserId();
        var profile = await _sellerService.GetProfileAsync(userId);
        return Ok(ApiResponse<SellerProfileDto>.Ok(profile));
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<SellerProfileDto>>> UpdateProfile([FromBody] UpdateSellerProfileDto dto)
    {
        var userId = User.GetUserId();
        var profile = await _sellerService.UpdateProfileAsync(userId, dto);
        return Ok(ApiResponse<SellerProfileDto>.Ok(profile, "Seller profile updated successfully."));
    }
}
