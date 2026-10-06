using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        ApplicationDbContext db,
        INotificationService notificationService,
        ILogger<PaymentService> logger)
    {
        _db = db;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentDto dto)
    {
        var order = await _db.Orders
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{dto.OrderId} not found.");
        }

        if (order.Payment == null)
        {
            order.Payment = new Payment
            {
                OrderId = order.Id,
                TransactionId = "TXN_" + Guid.NewGuid().ToString("N")[..12].ToUpper(),
                Amount = order.TotalAmount,
                Status = PaymentStatuses.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _db.Payments.Add(order.Payment);
        }

        if (order.Payment.Status == PaymentStatuses.Success)
        {
            throw new BadRequestException("This order has already been paid for successfully.");
        }

        if (dto.SimulateSuccess)
        {
            order.Payment.Status = PaymentStatuses.Success;
            order.Payment.PaidAt = DateTime.UtcNow;
            order.Status = OrderStatuses.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;

            // Reduce stock
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity = Math.Max(0, item.Product.StockQuantity - item.Quantity);
                    item.Product.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Payment of ₹{order.Payment.Amount:F2} for Order #{order.Id} was successful!");

            return MapPayment(order.Payment);
        }
        else
        {
            order.Payment.Status = PaymentStatuses.Failed;
            await _db.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Payment for Order #{order.Id} failed.");

            return MapPayment(order.Payment);
        }
    }

    public async Task<PaymentResponseDto> HandleWebhookAsync(PaymentWebhookDto dto)
    {
        _logger.LogInformation("Processing payment webhook event: {Event}, Transaction: {Txn}", dto.Event, dto.TransactionId);

        // Find payment by TransactionId or OrderId
        Payment? payment = null;
        if (!string.IsNullOrWhiteSpace(dto.TransactionId))
        {
            payment = await _db.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o!.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(p => p.TransactionId == dto.TransactionId);
        }

        if (payment == null && dto.OrderId.HasValue)
        {
            payment = await _db.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o!.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(p => p.OrderId == dto.OrderId.Value);
        }

        if (payment == null)
        {
            throw new NotFoundException($"Payment record not found for webhook transaction '{dto.TransactionId}'.");
        }

        var isSuccess = dto.Event.Contains("captured", StringComparison.OrdinalIgnoreCase) ||
                        dto.Event.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(dto.Status, "Success", StringComparison.OrdinalIgnoreCase);

        if (isSuccess && payment.Status != PaymentStatuses.Success)
        {
            payment.Status = PaymentStatuses.Success;
            payment.PaidAt = DateTime.UtcNow;

            if (payment.Order != null)
            {
                payment.Order.Status = OrderStatuses.Confirmed;
                payment.Order.UpdatedAt = DateTime.UtcNow;

                foreach (var item in payment.Order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity = Math.Max(0, item.Product.StockQuantity - item.Quantity);
                    }
                }

                await _notificationService.CreateNotificationAsync(
                    payment.Order.UserId,
                    $"Payment confirmed via webhook for Order #{payment.Order.Id}. Amount: ₹{payment.Amount:F2}.");
            }
        }
        else if (!isSuccess)
        {
            payment.Status = PaymentStatuses.Failed;
            if (payment.Order != null)
            {
                await _notificationService.CreateNotificationAsync(
                    payment.Order.UserId,
                    $"Payment failure reported via webhook for Order #{payment.Order.Id}.");
            }
        }

        await _db.SaveChangesAsync();
        return MapPayment(payment);
    }

    public async Task<PaymentResponseDto> GetPaymentByOrderIdAsync(int orderId)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        if (payment == null)
        {
            throw new NotFoundException($"Payment details not found for Order #{orderId}.");
        }

        return MapPayment(payment);
    }

    private static PaymentResponseDto MapPayment(Payment p)
    {
        return new PaymentResponseDto
        {
            Id = p.Id,
            OrderId = p.OrderId,
            TransactionId = p.TransactionId,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            PaidAt = p.PaidAt
        };
    }
}
