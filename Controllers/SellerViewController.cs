using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("seller")]
public class SellerViewController : Controller
{
    private readonly ISellerService _sellerService;
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _env;

    public SellerViewController(
        ISellerService sellerService,
        IProductService productService,
        IOrderService orderService,
        ICategoryService categoryService,
        IWebHostEnvironment env)
    {
        _sellerService = sellerService;
        _productService = productService;
        _orderService = orderService;
        _categoryService = categoryService;
        _env = env;
    }

    [HttpGet("apply")]
    public async Task<IActionResult> Apply()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Info"] = "Please sign in to apply for a seller shop.";
            return RedirectToAction("Login", "Account", new { returnUrl = "/seller/apply" });
        }

        int userId = User.GetUserId();
        try
        {
            var profile = await _sellerService.GetProfileAsync(userId);
            if (profile != null)
            {
                ViewBag.ExistingProfile = profile;
            }
        }
        catch
        {
            // No profile yet, continue
        }

        return View("~/Views/Seller/Apply.cshtml");
    }

    [HttpPost("apply")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply([FromForm] SellerApplicationDto dto)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Seller/Apply.cshtml", dto);
        }

        try
        {
            int userId = User.GetUserId();
            var profile = await _sellerService.ApplyAsync(userId, dto);
            TempData["Success"] = "Your vendor application has been submitted! It is currently pending review by an Administrator.";
            return RedirectToAction(nameof(Apply));
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("~/Views/Seller/Apply.cshtml", dto);
        }
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        int userId = User.GetUserId();
        var profile = await _sellerService.GetProfileAsync(userId);
        var products = await _productService.GetSellerProductsAsync(userId);
        var orders = await _orderService.GetSellerOrdersAsync(userId);

        ViewBag.SellerProfile = profile;
        ViewBag.Orders = orders;
        return View("~/Views/Seller/Dashboard.cshtml", products);
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpGet("products/new")]
    public async Task<IActionResult> NewProduct()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
        ViewBag.Categories = categories;
        ViewBag.IsEdit = false;
        return View("~/Views/Seller/ProductForm.cshtml", new CreateProductDto());
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpPost("products/new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewProduct([FromForm] CreateProductDto dto)
    {
        int userId = User.GetUserId();
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
            ViewBag.Categories = categories;
            ViewBag.IsEdit = false;
            return View("~/Views/Seller/ProductForm.cshtml", dto);
        }

        try
        {
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadedUrl = await ProcessUploadedImageAsync(dto.ImageFile);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    dto.ImageUrl = uploadedUrl;
                }
            }

            await _productService.CreateSellerProductAsync(userId, dto);
            TempData["Success"] = $"Product '{dto.Name}' has been added to your inventory!";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
            ViewBag.Categories = categories;
            ViewBag.IsEdit = false;
            return View("~/Views/Seller/ProductForm.cshtml", dto);
        }
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpGet("products/edit/{id:int}")]
    public async Task<IActionResult> EditProduct(int id)
    {
        int userId = User.GetUserId();
        try
        {
            var product = await _productService.GetSellerProductByIdAsync(userId, id);
            var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
            ViewBag.Categories = categories;
            ViewBag.IsEdit = true;
            ViewBag.ProductId = id;

            var dto = new UpdateProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            return View("~/Views/Seller/ProductForm.cshtml", dto);
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Dashboard));
        }
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpPost("products/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, [FromForm] UpdateProductDto dto)
    {
        int userId = User.GetUserId();
        if (!ModelState.IsValid)
        {
            var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
            ViewBag.Categories = categories;
            ViewBag.IsEdit = true;
            ViewBag.ProductId = id;
            return View("~/Views/Seller/ProductForm.cshtml", dto);
        }

        try
        {
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var uploadedUrl = await ProcessUploadedImageAsync(dto.ImageFile);
                if (!string.IsNullOrEmpty(uploadedUrl))
                {
                    dto.ImageUrl = uploadedUrl;
                }
            }
            else if (string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var existing = await _productService.GetSellerProductByIdAsync(userId, id);
                dto.ImageUrl = existing.ImageUrl;
            }

            await _productService.UpdateSellerProductAsync(userId, id, dto);
            TempData["Success"] = $"Product '{dto.Name}' updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);
            ViewBag.Categories = categories;
            ViewBag.IsEdit = true;
            ViewBag.ProductId = id;
            return View("~/Views/Seller/ProductForm.cshtml", dto);
        }
    }

    private async Task<string?> ProcessUploadedImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
        {
            throw new BadRequestException("Only JPG, JPEG, PNG, or WEBP image files are allowed.");
        }

        // Limit file size to 5MB
        if (file.Length > 5 * 1024 * 1024)
        {
            throw new BadRequestException("Uploaded image file size cannot exceed 5MB.");
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/products/{fileName}";
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpPost("products/delete/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        int userId = User.GetUserId();
        try
        {
            await _productService.DeleteSellerProductAsync(userId, id);
            TempData["Success"] = "Product deleted from your catalog.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize(Roles = UserRoles.Seller)]
    [HttpPost("orders/status")]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
    {
        int userId = User.GetUserId();
        try
        {
            await _orderService.UpdateSellerOrderStatusAsync(userId, orderId, status);
            TempData["Success"] = $"Order #{orderId} status updated to {status}.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
