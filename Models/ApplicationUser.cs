using Microsoft.AspNetCore.Identity;

namespace ECommerceApp.Models;

// Extends the default Identity user with a couple of extra profile fields.
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ShippingAddress { get; set; }

    // Navigation: each user has exactly one cart, created on first use.
    public Cart? Cart { get; set; }
}
