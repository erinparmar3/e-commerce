using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers;

[Authorize] // Cart requires a logged-in user, as requested.
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Ensures every user has exactly one cart row, creating it on first use.
    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    // GET: /Cart
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var cart = await GetOrCreateCartAsync(userId);
        return View(cart);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        if (quantity < 1) quantity = 1;

        var product = await _context.Products.FindAsync(productId);
        if (product == null || !product.IsActive)
        {
            TempData["Error"] = "That product is not available.";
            return RedirectToAction("Index", "Home");
        }

        var userId = _userManager.GetUserId(User)!;
        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        var currentQtyInCart = existingItem?.Quantity ?? 0;

        // Real stock check so the cart can't exceed available inventory.
        if (currentQtyInCart + quantity > product.StockQuantity)
        {
            TempData["Error"] = $"Only {product.StockQuantity} of \"{product.Name}\" available " +
                                 $"({currentQtyInCart} already in your cart).";
            return RedirectToAction("Details", "Home", new { id = productId });
        }

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            });
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Added \"{product.Name}\" to your cart.";
        return RedirectToAction("Index");
    }

    // POST: /Cart/UpdateQuantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var userId = _userManager.GetUserId(User)!;

        var item = await _context.CartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

        if (item == null)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            if (quantity > item.Product!.StockQuantity)
            {
                TempData["Error"] = $"Only {item.Product.StockQuantity} of \"{item.Product.Name}\" available.";
                quantity = item.Product.StockQuantity;
            }
            item.Quantity = quantity;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST: /Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        var userId = _userManager.GetUserId(User)!;

        var item = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart!.UserId == userId);

        if (item != null)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index");
    }

    // POST: /Cart/Checkout — converts the cart into a real Order, decrements stock, clears cart.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(string shippingAddress)
    {
        var userId = _userManager.GetUserId(User)!;
        var cart = await GetOrCreateCartAsync(userId);

        if (!cart.CartItems.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index");
        }

        // Re-validate stock at checkout time in case it changed since items were added.
        foreach (var item in cart.CartItems)
        {
            if (item.Product == null || item.Quantity > item.Product.StockQuantity)
            {
                TempData["Error"] = $"\"{item.Product?.Name}\" no longer has enough stock. Please update your cart.";
                return RedirectToAction("Index");
            }
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = string.IsNullOrWhiteSpace(shippingAddress) ? "Not provided" : shippingAddress,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Paid, // demo: assume payment succeeds
            TotalAmount = cart.Total
        };

        foreach (var item in cart.CartItems)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductNameSnapshot = item.Product!.Name,
                UnitPriceSnapshot = item.Product.Price,
                Quantity = item.Quantity
            });

            item.Product.StockQuantity -= item.Quantity;
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Order #{order.Id} placed successfully!";
        return RedirectToAction("Details", "Orders", new { id = order.Id });
    }
}
