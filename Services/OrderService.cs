using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly INotificationService _notificationService;

    public OrderService(ApplicationDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetBuyerOrdersAsync(int userId)
    {
        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderResponseDto> GetBuyerOrderByIdAsync(int userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{orderId} not found.");
        }

        if (order.UserId != userId)
        {
            throw new ForbiddenException("You cannot access another user's orders.");
        }

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{orderId} not found.");
        }

        if (order.UserId != userId)
        {
            throw new ForbiddenException("You cannot cancel another user's order.");
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            throw new BadRequestException("Order is already cancelled.");
        }

        if (order.Status == OrderStatuses.Shipped || order.Status == OrderStatuses.Delivered)
        {
            throw new BadRequestException($"Cannot cancel order in '{order.Status}' status. Cancellation is only allowed before shipment.");
        }

        var wasConfirmed = order.Status == OrderStatuses.Confirmed;
        order.Status = OrderStatuses.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // Restore inventory if it was confirmed and paid
        if (wasConfirmed)
        {
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }
        }

        await _db.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            userId,
            $"Order #{order.Id} has been cancelled successfully.");

        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetSellerOrdersAsync(int userId)
    {
        var seller = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (seller == null)
        {
            throw new ForbiddenException("User is not an approved seller.");
        }

        var orders = await _db.Orders
            .Where(o => o.OrderItems.Any(oi => oi.SellerId == seller.Id))
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Seller must only see order items belonging to that seller
        return orders.Select(o => MapToSellerDto(o, seller.Id));
    }

    public async Task<OrderResponseDto> UpdateSellerOrderStatusAsync(int userId, int orderId, string newStatus)
    {
        var seller = await _db.SellerProfiles.FirstOrDefaultAsync(s => s.UserId == userId);
        if (seller == null || seller.Status != SellerStatuses.Approved)
        {
            throw new ForbiddenException("User is not an approved seller.");
        }

        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{orderId} not found.");
        }

        var hasSellerItem = order.OrderItems.Any(oi => oi.SellerId == seller.Id);
        if (!hasSellerItem)
        {
            throw new ForbiddenException("You cannot manage an order that contains no products from your shop.");
        }

        ValidateStatusTransition(order.Status, newStatus);

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Send notifications based on new status
        if (newStatus == OrderStatuses.Shipped)
        {
            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Your order #{order.Id} has been shipped!");
        }
        else if (newStatus == OrderStatuses.Delivered)
        {
            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Your order #{order.Id} has been delivered. Enjoy your purchase!");
        }

        return MapToSellerDto(order, seller.Id);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAdminOrdersAsync()
    {
        var orders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToDto);
    }

    public async Task<OrderResponseDto> GetAdminOrderByIdAsync(int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{orderId} not found.");
        }

        return MapToDto(order);
    }

    public async Task<OrderResponseDto> UpdateAdminOrderStatusAsync(int orderId, string newStatus)
    {
        var order = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.Payment)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new NotFoundException($"Order #{orderId} not found.");
        }

        ValidateStatusTransition(order.Status, newStatus);

        var wasConfirmed = order.Status == OrderStatuses.Confirmed;
        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (newStatus == OrderStatuses.Cancelled && wasConfirmed)
        {
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                }
            }
        }

        await _db.SaveChangesAsync();

        if (newStatus == OrderStatuses.Shipped)
        {
            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Your order #{order.Id} has been shipped!");
        }
        else if (newStatus == OrderStatuses.Delivered)
        {
            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Your order #{order.Id} has been delivered.");
        }
        else if (newStatus == OrderStatuses.Cancelled)
        {
            await _notificationService.CreateNotificationAsync(
                order.UserId,
                $"Your order #{order.Id} was cancelled by administrator.");
        }

        return MapToDto(order);
    }

    private static void ValidateStatusTransition(string currentStatus, string nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return;
        }

        // Delivered -> Cancelled ❌, Cancelled -> Shipped ❌, Shipped -> Confirmed ❌
        if (currentStatus == OrderStatuses.Delivered)
        {
            throw new BadRequestException($"Cannot change status of an order that has already been delivered.");
        }

        if (currentStatus == OrderStatuses.Cancelled)
        {
            throw new BadRequestException($"Cannot change status of a cancelled order.");
        }

        if (currentStatus == OrderStatuses.Shipped && (nextStatus == OrderStatuses.Pending || nextStatus == OrderStatuses.Confirmed || nextStatus == OrderStatuses.Cancelled))
        {
            throw new BadRequestException($"Shipped order cannot be moved back to '{nextStatus}'.");
        }

        var validStatuses = new[]
        {
            OrderStatuses.Pending,
            OrderStatuses.Confirmed,
            OrderStatuses.Shipped,
            OrderStatuses.Delivered,
            OrderStatuses.Cancelled
        };

        if (!validStatuses.Contains(nextStatus))
        {
            throw new BadRequestException($"Invalid order status '{nextStatus}'.");
        }
    }

    private static OrderResponseDto MapToDto(Order o)
    {
        return new OrderResponseDto
        {
            Id = o.Id,
            UserId = o.UserId,
            CustomerName = o.User?.Name ?? o.FullName,
            TotalAmount = o.TotalAmount,
            FullName = o.FullName,
            Phone = o.Phone,
            ShippingAddress = o.ShippingAddress,
            City = o.City,
            State = o.State,
            PostalCode = o.PostalCode,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = o.OrderItems.Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                SellerId = oi.SellerId,
                SellerShopName = oi.Seller?.ShopName ?? string.Empty,
                PriceAtPurchase = oi.PriceAtPurchase,
                Quantity = oi.Quantity,
                Subtotal = oi.Subtotal
            }).ToList(),
            Payment = o.Payment != null ? new PaymentResponseDto
            {
                Id = o.Payment.Id,
                OrderId = o.Payment.OrderId,
                TransactionId = o.Payment.TransactionId,
                Amount = o.Payment.Amount,
                Status = o.Payment.Status,
                CreatedAt = o.Payment.CreatedAt,
                PaidAt = o.Payment.PaidAt
            } : null
        };
    }

    private static OrderResponseDto MapToSellerDto(Order o, int sellerId)
    {
        var sellerItems = o.OrderItems
            .Where(oi => oi.SellerId == sellerId)
            .Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                OrderId = oi.OrderId,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                SellerId = oi.SellerId,
                SellerShopName = oi.Seller?.ShopName ?? string.Empty,
                PriceAtPurchase = oi.PriceAtPurchase,
                Quantity = oi.Quantity,
                Subtotal = oi.Subtotal
            }).ToList();

        var sellerTotal = sellerItems.Sum(i => i.Subtotal);

        return new OrderResponseDto
        {
            Id = o.Id,
            UserId = o.UserId,
            CustomerName = o.User?.Name ?? o.FullName,
            TotalAmount = sellerTotal, // seller sees their items' share
            FullName = o.FullName,
            Phone = o.Phone,
            ShippingAddress = o.ShippingAddress,
            City = o.City,
            State = o.State,
            PostalCode = o.PostalCode,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = sellerItems,
            Payment = o.Payment != null ? new PaymentResponseDto
            {
                Id = o.Payment.Id,
                OrderId = o.Payment.OrderId,
                TransactionId = o.Payment.TransactionId,
                Amount = o.Payment.Amount,
                Status = o.Payment.Status,
                CreatedAt = o.Payment.CreatedAt,
                PaidAt = o.Payment.PaidAt
            } : null
        };
    }
}
