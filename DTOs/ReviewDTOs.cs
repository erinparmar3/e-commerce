using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.DTOs;

public class CreateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 3)]
    public string Comment { get; set; } = string.Empty;
}

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
