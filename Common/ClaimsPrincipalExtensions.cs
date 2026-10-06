using System.Security.Claims;

namespace ECommerceApp.Common;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedException("User ID could not be identified from authentication token.");
    }

    public static int? GetUserIdOrNull(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(claim, out var userId))
        {
            return userId;
        }

        return null;
    }

    public static string? GetUserEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? GetUserRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Role)?.Value;
    }
}
