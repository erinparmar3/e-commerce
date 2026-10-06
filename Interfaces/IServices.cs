using ECommerceApp.DTOs;

namespace ECommerceApp.Interfaces;

public interface IAuthService
{
    Task<UserProfileDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<string> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}

public interface ISellerService
{
    Task<SellerProfileDto> ApplyAsync(int userId, SellerApplicationDto dto);
    Task<SellerProfileDto> GetProfileAsync(int userId);
    Task<SellerProfileDto> UpdateProfileAsync(int userId, UpdateSellerProfileDto dto);
    Task<IEnumerable<SellerProfileDto>> GetPendingSellersAsync();
    Task<SellerProfileDto> ApproveSellerAsync(int sellerProfileId);
    Task<SellerProfileDto> RejectSellerAsync(int sellerProfileId);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool activeOnly = true);
    Task<CategoryResponseDto> GetCategoryByIdAsync(int id);
    Task<PaginatedResult<ProductResponseDto>> GetCategoryProductsAsync(int categoryId, ProductQueryParameters query);
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryResponseDto> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task<bool> DeleteCategoryAsync(int id);
}

public interface IProductService
{
    Task<PaginatedResult<ProductResponseDto>> GetProductsAsync(ProductQueryParameters query);
    Task<ProductResponseDto> GetProductByIdAsync(int id);
    Task<ProductResponseDto> CreateSellerProductAsync(int userId, CreateProductDto dto);
    Task<IEnumerable<ProductResponseDto>> GetSellerProductsAsync(int userId);
    Task<ProductResponseDto> GetSellerProductByIdAsync(int userId, int productId);
    Task<ProductResponseDto> UpdateSellerProductAsync(int userId, int productId, UpdateProductDto dto);
    Task<bool> DeleteSellerProductAsync(int userId, int productId);
}

public interface ICartService
{
    Task<CartResponseDto> GetCartAsync(int userId);
    Task<CartResponseDto> AddItemAsync(int userId, AddCartItemDto dto);
    Task<CartResponseDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto);
    Task<CartResponseDto> RemoveItemAsync(int userId, int cartItemId);
    Task<bool> ClearCartAsync(int userId);
}

public interface IWishlistService
{
    Task<IEnumerable<WishlistItemResponseDto>> GetWishlistAsync(int userId);
    Task<bool> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
}

public interface ICheckoutService
{
    Task<CheckoutResponseDto> CheckoutAsync(int userId, CheckoutRequestDto dto);
}

public interface IOrderService
{
    Task<IEnumerable<OrderResponseDto>> GetBuyerOrdersAsync(int userId);
    Task<OrderResponseDto> GetBuyerOrderByIdAsync(int userId, int orderId);
    Task<OrderResponseDto> CancelOrderAsync(int userId, int orderId);
    Task<IEnumerable<OrderResponseDto>> GetSellerOrdersAsync(int userId);
    Task<OrderResponseDto> UpdateSellerOrderStatusAsync(int userId, int orderId, string newStatus);
    Task<IEnumerable<OrderResponseDto>> GetAdminOrdersAsync();
    Task<OrderResponseDto> GetAdminOrderByIdAsync(int orderId);
    Task<OrderResponseDto> UpdateAdminOrderStatusAsync(int orderId, string newStatus);
}

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(ProcessPaymentDto dto);
    Task<PaymentResponseDto> HandleWebhookAsync(PaymentWebhookDto dto);
    Task<PaymentResponseDto> GetPaymentByOrderIdAsync(int orderId);
}

public interface IReviewService
{
    Task<IEnumerable<ReviewResponseDto>> GetProductReviewsAsync(int productId);
    Task<ReviewResponseDto> CreateReviewAsync(int userId, int productId, CreateReviewDto dto);
    Task<ReviewResponseDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto);
    Task<bool> DeleteReviewAsync(int userId, int reviewId);
}

public interface INotificationService
{
    Task<IEnumerable<NotificationResponseDto>> GetUserNotificationsAsync(int userId);
    Task<bool> MarkAsReadAsync(int userId, int notificationId);
    Task CreateNotificationAsync(int userId, string message);
}

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync();
    Task<IEnumerable<AdminUserDto>> GetUsersAsync();
    Task<AdminUserDto> UpdateUserStatusAsync(int userId, bool isActive);
}
