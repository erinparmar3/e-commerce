using System.Security.Claims;
using ECommerceApp.Common;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromForm] LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var result = await _authService.LoginAsync(dto);

            // Establish Cookie session
            await SignInUserCookieAsync(result.User);

            TempData["Success"] = $"Welcome back, {result.User.Name}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (result.User.Role == UserRoles.Admin)
            {
                return Redirect("/admin/dashboard");
            }
            if (result.User.Role == UserRoles.Seller)
            {
                return Redirect("/seller/dashboard");
            }

            return RedirectToAction("Index", "Home");
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An error occurred during authentication.");
            return View(dto);
        }
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([FromForm] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var user = await _authService.RegisterAsync(dto);
            await SignInUserCookieAsync(user);

            TempData["Success"] = $"Account created successfully! Welcome to ShopNet, {user.Name}.";
            return RedirectToAction("Index", "Home");
        }
        catch (AppException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Could not register account. Please check your information and try again.");
            return View(dto);
        }
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Info"] = "You have been signed out successfully.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        try
        {
            int userId = User.GetUserId();
            var profile = await _authService.GetProfileAsync(userId);
            return View(profile);
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [Authorize]
    [HttpPost("profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile([FromForm] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            int userId = User.GetUserId();
            var updated = await _authService.UpdateProfileAsync(userId, dto);
            
            // Refresh cookie with updated name
            await SignInUserCookieAsync(updated);

            TempData["Success"] = "Your profile information has been updated.";
            return RedirectToAction(nameof(Profile));
        }
        catch (AppException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Profile));
        }
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserCookieAsync(UserProfileDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            });
    }
}
