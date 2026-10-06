using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class ProcessPaymentDto
{
    [Required]
    public int OrderId { get; set; }

    public bool SimulateSuccess { get; set; } = true;
}

public class PaymentWebhookDto
{
    [Required]
    public string Event { get; set; } = string.Empty; // e.g. "payment.captured", "payment.failed"

    [Required]
    public string TransactionId { get; set; } = string.Empty;

    public int? OrderId { get; set; }
    public decimal? Amount { get; set; }
    public string? Status { get; set; }
    public string? Signature { get; set; }
}
