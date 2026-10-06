namespace ECommerceApp.Common;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "SuperSecretDefaultDevelopmentKeyForJWTAuthentication2026!@#$";
    public string Issuer { get; set; } = "ECommerceApp";
    public string Audience { get; set; } = "ECommerceAppBuyersSellers";
    public int ExpiryMinutes { get; set; } = 60; // 1 hour as requested
}
