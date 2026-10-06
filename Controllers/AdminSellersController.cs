using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/admin/sellers")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminSellersController : ControllerBase
{
    private readonly ISellerService _sellerService;

    public AdminSellersController(ISellerService sellerService)
    {
        _sellerService = sellerService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SellerProfileDto>>>> GetPending()
    {
        var pendingSellers = await _sellerService.GetPendingSellersAsync();
        return Ok(ApiResponse<IEnumerable<SellerProfileDto>>.Ok(pendingSellers));
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<ApiResponse<SellerProfileDto>>> Approve([FromRoute] int id)
    {
        var profile = await _sellerService.ApproveSellerAsync(id);
        return Ok(ApiResponse<SellerProfileDto>.Ok(profile, "Seller has been approved successfully."));
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult<ApiResponse<SellerProfileDto>>> Reject([FromRoute] int id)
    {
        var profile = await _sellerService.RejectSellerAsync(id);
        return Ok(ApiResponse<SellerProfileDto>.Ok(profile, "Seller application has been rejected."));
    }
}
