using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Route("admin")]
public class AdminViewController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ISellerService _sellerService;
    private readonly ICategoryService _categoryService;
    private readonly IOrderService _orderService;

    public AdminViewController(
        IAdminService adminService,
        ISellerService sellerService,
        ICategoryService categoryService,
        IOrderService orderService)
    {
        _adminService = adminService;
        _sellerService = sellerService;
        _categoryService = categoryService;
        _orderService = orderService;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        var pendingSellers = await _sellerService.GetPendingSellersAsync();
        var orders = await _orderService.GetAdminOrdersAsync();

        ViewBag.PendingSellers = pendingSellers;
        ViewBag.RecentOrders = orders.Take(10);

        return View("~/Views/Admin/Index.cshtml", stats);
    }

    [HttpGet("sellers")]
    public async Task<IActionResult> Sellers()
    {
        var pendingSellers = await _sellerService.GetPendingSellersAsync();
        return View("~/Views/Admin/Sellers.cshtml", pendingSellers);
    }

    [HttpPost("sellers/approve/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveSeller(int id)
    {
        try
        {
            var profile = await _sellerService.ApproveSellerAsync(id);
            TempData["Success"] = $"Seller '{profile.ShopName}' ({profile.OwnerEmail}) has been approved and granted Seller status!";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Sellers));
    }

    [HttpPost("sellers/reject/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectSeller(int id)
    {
        try
        {
            var profile = await _sellerService.RejectSellerAsync(id);
            TempData["Info"] = $"Seller application for '{profile.ShopName}' has been rejected.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Sellers));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: false);
        return View("~/Views/Admin/Categories.cshtml", categories);
    }

    [HttpPost("categories/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory([FromForm] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Category name is required.";
            return RedirectToAction(nameof(Categories));
        }

        try
        {
            await _categoryService.CreateCategoryAsync(dto);
            TempData["Success"] = $"Category '{dto.Name}' created successfully.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Categories));
    }

    [HttpPost("categories/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(id);
            TempData["Success"] = "Category deleted successfully.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Categories));
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var users = await _adminService.GetUsersAsync();
        return View("~/Views/Admin/Users.cshtml", users);
    }

    [HttpPost("users/status/{userId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserStatus(int userId, [FromForm] bool isActive)
    {
        try
        {
            var user = await _adminService.UpdateUserStatusAsync(userId, isActive);
            TempData["Success"] = $"User '{user.Name}' ({user.Email}) is now {(isActive ? "Active" : "Deactivated")}.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var orders = await _orderService.GetAdminOrdersAsync();
        return View("~/Views/Admin/Orders.cshtml", orders);
    }
}
