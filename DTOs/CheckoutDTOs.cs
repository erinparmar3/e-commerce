using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class CheckoutRequestDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(300, MinimumLength = 5)]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string PostalCode { get; set; } = string.Empty;

    // Optional simulated payment method (e.g. "CARD", "UPI", "NETBANKING")
    public string? PaymentMethod { get; set; } = "CARD";
}

public class CheckoutResponseDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
