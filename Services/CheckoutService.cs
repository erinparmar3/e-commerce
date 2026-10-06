using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;
    private const decimal TaxRate = 0.10m; // 10% tax

    public CheckoutService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<CheckoutResponseDto> CheckoutAsync(int userId, CheckoutRequestDto dto)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p!.Seller)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            throw new BadRequestException("Your shopping cart is empty.");
        }

        // 1. Verify all products are active and check stock
        foreach (var item in cart.CartItems)
        {
            if (item.Product == null || !item.Product.IsActive)
            {
                throw new BadRequestException($"Product '{(item.Product?.Name ?? "Item")}' is no longer active or available.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                throw new BadRequestException($"Insufficient stock for '{item.Product.Name}'. Requested: {item.Quantity}, Available: {item.Product.StockQuantity}.");
            }
        }

        // 2. Fetch current prices from database and calculate subtotal & total
        decimal subtotal = 0m;
        var orderItems = new List<OrderItem>();

        foreach (var item in cart.CartItems)
        {
            var currentPrice = item.Product!.Price;
            var itemSubtotal = currentPrice * item.Quantity;
            subtotal += itemSubtotal;

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                SellerId = item.Product.SellerId,
                ProductName = item.Product.Name,
                PriceAtPurchase = currentPrice,
                Quantity = item.Quantity,
                Subtotal = itemSubtotal
            });
        }

        var tax = Math.Round(subtotal * TaxRate, 2);
        var total = subtotal + tax;

        // 3. Create Order
        var order = new Order
        {
            UserId = userId,
            TotalAmount = total,
            FullName = dto.FullName.Trim(),
            Phone = dto.Phone.Trim(),
            ShippingAddress = dto.ShippingAddress.Trim(),
            City = dto.City.Trim(),
            State = dto.State.Trim(),
            PostalCode = dto.PostalCode.Trim(),
            Status = OrderStatuses.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = orderItems
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // 4. Create Payment record
        var txnId = "TXN_" + Guid.NewGuid().ToString("N")[..12].ToUpper();
        var isSimulatedFailure = string.Equals(dto.PaymentMethod, "FAIL", StringComparison.OrdinalIgnoreCase);

        var payment = new Payment
        {
            OrderId = order.Id,
            TransactionId = txnId,
            Amount = total,
            Status = isSimulatedFailure ? PaymentStatuses.Failed : PaymentStatuses.Success,
            CreatedAt = DateTime.UtcNow,
            PaidAt = isSimulatedFailure ? null : DateTime.UtcNow
        };

        _db.Payments.Add(payment);

        if (isSimulatedFailure)
        {
            // Payment failed - do not confirm order, do not reduce inventory, do not clear cart
            await _db.SaveChangesAsync();
            await _notificationService.CreateNotificationAsync(
                userId,
                $"Payment failed for Order #{order.Id}. Amount: ₹{total:F2}.");

            return new CheckoutResponseDto
            {
                OrderId = order.Id,
                TotalAmount = total,
                OrderStatus = order.Status,
                PaymentStatus = payment.Status,
                TransactionId = txnId,
                Message = "Payment processing failed. Your order is pending payment."
            };
        }

        // Successful Payment Flow
        order.Status = OrderStatuses.Confirmed;
        order.UpdatedAt = DateTime.UtcNow;

        // Reduce inventory
        foreach (var item in cart.CartItems)
        {
            item.Product!.StockQuantity -= item.Quantity;
            item.Product.UpdatedAt = DateTime.UtcNow;
        }

        // Clear cart
        _db.CartItems.RemoveRange(cart.CartItems);
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Notify Buyer
        await _notificationService.CreateNotificationAsync(
            userId,
            $"Order #{order.Id} confirmed! Payment of ₹{total:F2} was successful. We are processing your shipment.");

        // Notify each unique Seller who has items in this order
        var sellerIds = orderItems.Select(oi => oi.SellerId).Distinct();
        foreach (var sellerId in sellerIds)
        {
            var sellerProfile = await _db.SellerProfiles.FindAsync(sellerId);
            if (sellerProfile != null)
            {
                await _notificationService.CreateNotificationAsync(
                    sellerProfile.UserId,
                    $"New Order #{order.Id} received for shop '{sellerProfile.ShopName}'.");
            }
        }

        return new CheckoutResponseDto
        {
            OrderId = order.Id,
            TotalAmount = total,
            OrderStatus = order.Status,
            PaymentStatus = payment.Status,
            TransactionId = txnId,
            Message = "Checkout and payment successful! Order confirmed."
        };
    }
}
