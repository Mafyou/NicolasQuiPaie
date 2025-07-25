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
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                logger.LogWarning("Login attempt with empty email from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Email is required"]
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                logger.LogWarning("Login attempt with empty password for email {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Password is required"]
                });
            }

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                logger.LogWarning("Login attempt with non-existent email: {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                });
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed login attempt for user {UserId} ({Email}) from IP: {ClientIP}", 
                    user.Id, request.Email, clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid email or password"]
                });
            }

            var token = jwtService.GenerateToken(user);
            var refreshToken = jwtService.GenerateRefreshToken();

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

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
            logger.LogError(ex, "Critical error during login for email {Email} from IP: {ClientIP}", 
                request.Email, clientIp);
            return Results.Problem("Login failed");
        }
    }

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequestDto request,
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                logger.LogWarning("Registration attempt with empty email from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Email is required"]
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                logger.LogWarning("Registration attempt with empty password for email {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Password is required"]
                });
            }

            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser is not null)
            {
                logger.LogWarning("Registration attempt with existing email: {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
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
                logger.LogError("User creation failed for email {Email} from IP: {ClientIP}. Errors: {Errors}", 
                    request.Email, clientIp, string.Join(", ", result.Errors.Select(e => e.Description)));
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
            logger.LogError(ex, "Critical error during registration for email {Email} from IP: {ClientIP}", 
                request.Email, clientIp);
            return Results.Problem("Registration failed");
        }
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequestDto request,
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            // Validate the refresh token format
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                logger.LogWarning("Token refresh attempt with empty refresh token from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid refresh token"]
                });
            }

            if (string.IsNullOrEmpty(request.UserId))
            {
                logger.LogWarning("Token refresh attempt with empty user ID from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["User ID required for token refresh"]
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

                var user = await userManager.FindByIdAsync(request.UserId);
                if (user is null)
                {
                    logger.LogWarning("Token refresh attempt for non-existent user: {UserId} from IP: {ClientIP}", 
                        request.UserId, clientIp);
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
            catch (FormatException ex)
            {
                logger.LogWarning(ex, "Token refresh attempt with invalid token format from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Errors = ["Invalid refresh token format"]
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error during token refresh for user {UserId} from IP: {ClientIP}", 
                request.UserId, clientIp);
            return Results.Problem("Token refresh failed");
        }
    }

    // C# 13.0 - Lambda expression with static modifier
    private static Task<IResult> LogoutAsync(
        [FromServices] ILogger<Program> logger,
        HttpContext context) =>
        // With JWT, logout is handled client-side by removing the token
        Task.FromResult(Results.Ok(new { message = "Logged out successfully" }));

    private static async Task<IResult> ForgotPasswordAsync(
        [FromBody] PasswordResetRequestDto request,
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                logger.LogWarning("Password reset attempt with empty email from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new { message = "Email is required" });
            }

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                // For security, don't reveal if email exists or not, but log the attempt
                logger.LogWarning("Password reset attempt for non-existent email: {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
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
            logger.LogError(ex, "Critical error processing password reset for email {Email} from IP: {ClientIP}", 
                request.Email, clientIp);
            return Results.Problem("Error processing password reset");
        }
    }

    private static async Task<IResult> ResetPasswordAsync(
        [FromBody] PasswordResetConfirmDto request,
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                logger.LogWarning("Password reset confirmation with empty email from IP: {ClientIP}", clientIp);
                return Results.BadRequest(new { message = "Email is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                logger.LogWarning("Password reset confirmation with empty token for email {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new { message = "Reset token is required" });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                logger.LogWarning("Password reset confirmation with empty password for email {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new { message = "New password is required" });
            }

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                logger.LogWarning("Password reset attempt for non-existent user: {Email} from IP: {ClientIP}", 
                    request.Email, clientIp);
                return Results.BadRequest(new { message = "Invalid reset request" });
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

            if (result.Succeeded)
            {
                // Warning level for password reset as it's a security-sensitive action
                logger.LogInformation("Password reset successful for user {UserId} ({Email}) from IP: {ClientIP}", 
                    user.Id, request.Email, clientIp);
                return Results.Ok(new { message = "Password reset successfully" });
            }
            else
            {
                logger.LogError("Password reset failed for user {UserId} ({Email}) from IP: {ClientIP}. Errors: {Errors}", 
                    user.Id, request.Email, clientIp, string.Join(", ", result.Errors.Select(e => e.Description)));
                return Results.BadRequest(new
                {
                    message = "Password reset failed",
                    errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error resetting password for email {Email} from IP: {ClientIP}", 
                request.Email, clientIp);
            return Results.Problem("Error resetting password");
        }
    }
}