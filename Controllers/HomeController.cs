using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;
    private readonly IWishlistService _wishlistService;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IReviewService reviewService,
        IWishlistService wishlistService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _wishlistService = wishlistService;
    }

    [HttpGet("")]
    [HttpGet("home/index")]
    public async Task<IActionResult> Index([FromQuery] ProductQueryParameters query)
    {
        var products = await _productService.GetProductsAsync(query);
        var categories = await _categoryService.GetAllCategoriesAsync(activeOnly: true);

        var wishlistIds = new HashSet<int>();
        if (User.Identity?.IsAuthenticated == true)
        {
            try
            {
                int userId = User.GetUserId();
                var wishlist = await _wishlistService.GetWishlistAsync(userId);
                wishlistIds = wishlist.Select(w => w.ProductId).ToHashSet();
            }
            catch { }
        }

        ViewBag.Categories = categories;
        ViewBag.CurrentQuery = query;
        ViewBag.WishlistIds = wishlistIds;

        return View(products);
    }

    [HttpGet("product/{id:int}")]
    [HttpGet("home/details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            var reviews = await _reviewService.GetProductReviewsAsync(id);

            bool isInWishlist = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    int userId = User.GetUserId();
                    var wishlist = await _wishlistService.GetWishlistAsync(userId);
                    isInWishlist = wishlist.Any(w => w.ProductId == id);
                }
                catch { }
            }

            ViewBag.Reviews = reviews;
            ViewBag.IsInWishlist = isInWishlist;
            return View(product);
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("product/{id:int}/review")]
    [HttpPost("home/addreview/{id:int}")]
    [HttpPost("home/addreview")]
    public async Task<IActionResult> AddReview(int id, [FromForm] CreateReviewDto dto)
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            TempData["Error"] = "Please sign in to write a review.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            int userId = User.GetUserId();
            await _reviewService.CreateReviewAsync(userId, id, dto);
            TempData["Success"] = "Thank you! Your verified review has been submitted.";
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not submit review. Note: You can only review products you have purchased and had delivered.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
