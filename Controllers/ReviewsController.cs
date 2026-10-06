using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("api/products/{productId}/reviews")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ReviewResponseDto>>>> GetProductReviews([FromRoute] int productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(ApiResponse<IEnumerable<ReviewResponseDto>>.Ok(reviews));
    }

    [Authorize]
    [HttpPost("api/products/{productId}/reviews")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> CreateReview(
        [FromRoute] int productId,
        [FromBody] CreateReviewDto dto)
    {
        var userId = User.GetUserId();
        var review = await _reviewService.CreateReviewAsync(userId, productId, dto);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ReviewResponseDto>.Ok(review, "Review submitted successfully."));
    }

    [Authorize]
    [HttpPut("api/reviews/{id}")]
    public async Task<ActionResult<ApiResponse<ReviewResponseDto>>> UpdateReview(
        [FromRoute] int id,
        [FromBody] UpdateReviewDto dto)
    {
        var userId = User.GetUserId();
        var review = await _reviewService.UpdateReviewAsync(userId, id, dto);
        return Ok(ApiResponse<ReviewResponseDto>.Ok(review, "Review updated successfully."));
    }

    [Authorize]
    [HttpDelete("api/reviews/{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteReview([FromRoute] int id)
    {
        var userId = User.GetUserId();
        var result = await _reviewService.DeleteReviewAsync(userId, id);
        return Ok(ApiResponse<bool>.Ok(result, "Review deleted successfully."));
    }
}
