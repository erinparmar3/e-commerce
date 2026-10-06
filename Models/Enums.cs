namespace ECommerceApp.Models;

public static class UserRoles
{
    public const string Admin = "ADMIN";
    public const string Buyer = "BUYER";
    public const string Seller = "SELLER";
}

public static class SellerStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Suspended = "Suspended";
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}

public static class PaymentStatuses
{
    public const string Pending = "Pending";
    public const string Success = "Success";
    public const string Failed = "Failed";
}
