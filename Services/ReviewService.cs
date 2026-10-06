using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;

    public ReviewService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetProductReviewsAsync(int productId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            UserId = r.UserId,
            UserName = r.User?.Name ?? "Anonymous",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(int userId, int productId, CreateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new BadRequestException("Rating must be between 1 and 5.");
        }

        var product = await _db.Products.FindAsync(productId);
        if (product == null)
        {
            throw new NotFoundException($"Product with ID {productId} not found.");
        }

        // Rule: User must have purchased the product in a confirmed, shipped, or delivered order
        var orderItem = await _db.OrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi =>
                oi.ProductId == productId &&
                oi.Order!.UserId == userId &&
                (oi.Order.Status == OrderStatuses.Confirmed ||
                 oi.Order.Status == OrderStatuses.Shipped ||
                 oi.Order.Status == OrderStatuses.Delivered));

        if (orderItem == null)
        {
            throw new ForbiddenException("You can only review products that you have purchased.");
        }

        var existingReview = await _db.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

        if (existingReview != null)
        {
            throw new ConflictException("You have already reviewed this product. You can update your existing review.");
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            OrderItemId = orderItem.Id,
            Rating = dto.Rating,
            Comment = dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);

        return new ReviewResponseDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            UserName = user?.Name ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<ReviewResponseDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new BadRequestException("Rating must be between 1 and 5.");
        }

        var review = await _db.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only edit your own reviews.");
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new ReviewResponseDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            UserId = review.UserId,
            UserName = review.User?.Name ?? string.Empty,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
    {
        var review = await _db.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            throw new NotFoundException("Review not found.");
        }

        if (review.UserId != userId)
        {
            throw new ForbiddenException("You can only delete your own reviews.");
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }
}
