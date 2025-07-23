using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NicolasQuiPaieData.DTOs;
using NicolasQuiPaieAPI.Infrastructure.Models;
using NicolasQuiPaieAPI.Application.Interfaces;

namespace NicolasQuiPaieAPI.Presentation.Endpoints;

/// <summary>
/// C# 13.0 - Authentication endpoints with contribution-based user levels
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/login", LoginAsync)
            .WithSummary("User login")
            .WithDescription("Authenticate user and return JWT token with contribution level");

        group.MapPost("/register", RegisterAsync)
            .WithSummary("User registration")
            .WithDescription("Register new user account with initial contribution level");

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithSummary("Refresh JWT token")
            .WithDescription("Refresh expired JWT token using refresh token");

        group.MapPost("/logout", LogoutAsync)
            .WithSummary("User logout")
            .WithDescription("Invalidate user session");

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithSummary("Password reset request")
            .WithDescription("Send password reset email");

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithSummary("Reset password")
            .WithDescription("Reset user password with token");
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequestDto request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                });
            }

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            return Results.Ok(new AuthResponseDto
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    ContributionLevel = (NicolasQuiPaieData.DTOs.ContributionLevel)(int)user.ContributionLevel,
                    ReputationScore = user.ReputationScore,
                    IsVerified = user.IsVerified,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = [$"Login failed: {ex.Message}"]
            });
        }
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequestDto request,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService)
    {
        try
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Email already exists"]
                });
            }

            // C# 13.0 - Object initialization with contribution level
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                Bio = request.Bio,
                ContributionLevel = Infrastructure.Models.ContributionLevel.PetitNicolas, // Start at basic level
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true, // For simplicity, auto-confirm emails
                IsVerified = false,
                ReputationScore = 0
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            return Results.Ok(new AuthResponseDto
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    ContributionLevel = (NicolasQuiPaieData.DTOs.ContributionLevel)(int)user.ContributionLevel,
                    ReputationScore = user.ReputationScore,
                    IsVerified = user.IsVerified,
                    CreatedAt = user.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = [$"Registration failed: {ex.Message}"]
            });
        }
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequestDto request,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService)
    {
        try
        {
            // Validate the refresh token format
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid refresh token"]
                });
            }

            // In a production environment, you would:
            // 1. Store refresh tokens in database with expiry
            // 2. Validate the refresh token against stored tokens
            // 3. Check if token is revoked or expired
            
            // For now, we'll implement a basic validation
            // This is a simplified implementation - in production you'd use proper token storage
            try
            {
                var tokenBytes = Convert.FromBase64String(request.RefreshToken);
                var tokenGuid = new Guid(tokenBytes);
                
                // For demo purposes, we'll assume the token is valid if it's a valid GUID
                // In production, lookup this token in the database
                
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return Results.BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Errors = ["User ID required for token refresh"]
                    });
                }

                var user = await userManager.FindByIdAsync(request.UserId);
                if (user is null)
                {
                    return Results.BadRequest(new AuthResponseDto
                    {
                        Success = false,
                        Errors = ["User not found"]
                    });
                }

                // Generate new tokens
                var newToken = jwtService.GenerateToken(user);
                var newRefreshToken = jwtService.GenerateRefreshToken();

                return Results.Ok(new AuthResponseDto
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        DisplayName = user.DisplayName,
                        ContributionLevel = (NicolasQuiPaieData.DTOs.ContributionLevel)(int)user.ContributionLevel,
                        ReputationScore = user.ReputationScore,
                        IsVerified = user.IsVerified,
                        CreatedAt = user.CreatedAt
                    }
                });
            }
            catch (FormatException)
            {
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid refresh token format"]
                });
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new AuthResponseDto
            {
                Success = false,
                Errors = [$"Token refresh failed: {ex.Message}"]
            });
        }
    }

    // C# 13.0 - Lambda expression with static modifier
    private static Task<IResult> LogoutAsync() =>
        // With JWT, logout is handled client-side by removing the token
        Task.FromResult(Results.Ok(new { message = "Logged out successfully" }));

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] PasswordResetRequestDto request,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                // For security, don't reveal if email exists or not
                return Results.Ok(new { message = "If the email exists, a password reset link has been sent" });
            }

            // Generate password reset token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create reset link (in production, this would be your app's URL)
            var resetLink = $"https://localhost:7084/reset-password?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email ?? "")}";
            
            // Send email
            await emailService.SendPasswordResetEmailAsync(user.Email ?? "", resetLink);
            
            return Results.Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = $"Error processing password reset: {ex.Message}" });
        }
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] PasswordResetConfirmDto request,
        UserManager<ApplicationUser> userManager)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Results.BadRequest(new { message = "Invalid reset request" });
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            
            if (result.Succeeded)
            {
                return Results.Ok(new { message = "Password reset successfully" });
            }
            else
            {
                return Results.BadRequest(new { 
                    message = "Password reset failed", 
                    errors = result.Errors.Select(e => e.Description).ToList() 
                });
            }
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { message = $"Error resetting password: {ex.Message}" });
        }
    }
}