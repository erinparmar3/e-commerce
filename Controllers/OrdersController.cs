using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponseDto>>>> GetBuyerOrders()
    {
        var userId = User.GetUserId();
        var orders = await _orderService.GetBuyerOrdersAsync(userId);
        return Ok(ApiResponse<IEnumerable<OrderResponseDto>>.Ok(orders));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> GetBuyerOrderById([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var order = await _orderService.GetBuyerOrderByIdAsync(userId, id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order));
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderResponseDto>>> CancelOrder([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var order = await _orderService.CancelOrderAsync(userId, id);
        return Ok(ApiResponse<OrderResponseDto>.Ok(order, "Order has been cancelled successfully."));
    }
}
