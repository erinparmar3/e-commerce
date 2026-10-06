using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetAll()
    {
        var orders = await _orderService.GetAdminOrdersAsync();
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.Ok(orders));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetById([FromRoute] int id)
    {
        var order = await _orderService.GetAdminOrderByIdAsync(id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order));
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> UpdateStatus(
        [FromRoute] int id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _orderService.UpdateAdminOrderStatusAsync(id, dto.Status);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, $"Order #{id} status updated to {dto.Status}."));
    }
}
