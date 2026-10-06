using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost("process")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Process([FromBody] ProcessPaymentDto dto)
    {
        var result = await _paymentService.ProcessPaymentAsync(dto);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(result, "Payment processed."));
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Webhook([FromBody] PaymentWebhookDto dto)
    {
        var result = await _paymentService.HandleWebhookAsync(dto);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(result, "Webhook handled successfully."));
    }

    [Authorize]
    [HttpGet("{orderId}")]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> GetByOrderId([FromRoute] int orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(payment));
    }
}
