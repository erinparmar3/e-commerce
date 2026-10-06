using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/seller/orders")]
[Authorize(Roles = $"{UserRoles.Seller},{UserRoles.Admin}")]
public class SellerOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public SellerOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetSellerOrders()
    {
        var userId = User.GetUserId();
        var orders = await _orderService.GetSellerOrdersAsync(userId);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.Ok(orders));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateStatus(
        [FromRoute] int id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        var userId = User.GetUserId();
        var order = await _orderService.UpdateSellerOrderStatusAsync(userId, id, dto.Status);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, $"Order #{id} status updated to {dto.Status}."));
    }
}
