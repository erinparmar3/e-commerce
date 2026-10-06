using System.Security.Cryptography;
using ECommerceApp.Common;
using ECommerceApp.Data;
using ECommerceApp.DTOs;
using ECommerceApp.Interfaces;
using ECommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existingUser = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (existingUser)
        {
            throw new ConflictException("A user with this email address already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            Phone = dto.Phone?.Trim(),
            Role = UserRoles.Buyer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Cart = new Cart
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is inactive. Please contact support.");
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
        {
            throw new NotFoundException("No account found with this email address.");
        }

        // Invalidate older unused tokens
        var existingTokens = await _db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.IsUsed)
            .ToListAsync();
        foreach (var t in existingTokens)
        {
            t.IsUsed = true;
        }

        // Generate 32-character secure token
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Password reset token generated for user {Email}: {Token}", user.Email, token);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
        {
            throw new BadRequestException("Invalid reset token or email.");
        }

        var tokenRecord = await _db.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == dto.Token && !t.IsUsed);

        if (tokenRecord == null || tokenRecord.ExpiresAt < DateTime.UtcNow)
        {
            throw new BadRequestException("Invalid or expired password reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        tokenRecord.IsUsed = true;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        user.Name = dto.Name.Trim();
        user.Phone = dto.Phone?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
