namespace NicolasQuiPaieAPI.Presentation.Endpoints;

/// <summary>
/// Authentication endpoints with contribution-based user levels
/// </summary>
public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi()
            .AllowAnonymous();

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

        // 🎯 New anonymous endpoint to check user role
        group.MapGet("/check-role", CheckUserRoleAsync)
            .WithSummary("Check current user role")
            .WithDescription("Anonymous endpoint to check what role the current user has (useful for debugging)")
            .AllowAnonymous();
    }

    /// <summary>
    /// C# 13.0 - Anonymous endpoint to check current user role and authentication state
    /// </summary>
    private static async Task<IResult> CheckUserRoleAsync(
        [FromServices] ILogger<Program> logger,
        [FromServices] IJwtService jwtService,
        HttpContext context)
    {
        var clientIp = $"{context.Connection.RemoteIpAddress}";

        try
        {
            // Get the Authorization header
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (string.IsNullOrEmpty(authHeader))
            {
                logger.LogDebug("Role check request with no Authorization header from IP: {ClientIP}", clientIp);
                return Results.Ok(new UserRoleCheckDto
                {
                    IsAuthenticated = false,
                    Message = "No authorization header provided",
                    Roles = [],
                    Email = null,
                    HighestRole = "Anonymous",
                    Claims = [],
                    DeveloperEmailAccess = false
                });
            }

            // Extract token from "Bearer <token>" format
            var token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader[7..]
                : authHeader;

            if (string.IsNullOrEmpty(token))
            {
                logger.LogDebug("Role check request with empty token from IP: {ClientIP}", clientIp);
                return Results.Ok(new UserRoleCheckDto
                {
                    IsAuthenticated = false,
                    Message = "No token provided in Authorization header",
                    Roles = [],
                    Email = null,
                    HighestRole = "Anonymous",
                    Claims = [],
                    DeveloperEmailAccess = false
                });
            }

            // Validate the token and get principal
            var principal = jwtService.ValidateToken(token);

            if (principal is null)
            {
                logger.LogWarning("Role check request with invalid token from IP: {ClientIP}", clientIp);
                return Results.Ok(new UserRoleCheckDto
                {
                    IsAuthenticated = false,
                    Message = "Invalid or expired token",
                    Roles = [],
                    Email = null,
                    HighestRole = "Anonymous",
                    Claims = [],
                    DeveloperEmailAccess = false
                });
            }

            // Extract user information from claims
            var roleClaims = principal.FindAll(ClaimTypes.Role).ToList();
            var simpleRoleClaims = principal.FindAll("role").ToList();
            var roles = roleClaims.Count > 0
                ? roleClaims.Select(c => c.Value).ToList()
                : simpleRoleClaims.Select(c => c.Value).ToList();
            var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
            var displayName = principal.FindFirstValue("DisplayName");
            var contributionLevel = principal.FindFirstValue("ContributionLevel");

            // Get highest role using the service
            var highestRole = jwtService.GetHighestRole(token);

            // Check for special developer email access
            var isDeveloperEmail = string.Equals(email, "Sata77@gmail.com", StringComparison.OrdinalIgnoreCase);

            // Get all claims for debugging
            var allClaims = principal.Claims
                .Select(c => new ClaimInfo { Type = c.Type, Value = c.Value })
                .ToList();

            logger.LogInformation("Role check successful for user {Email} with roles: {Roles} from IP: {ClientIP}",
                email, string.Join(", ", roles), clientIp);

            return Results.Ok(new UserRoleCheckDto
            {
                IsAuthenticated = true,
                Message = "Token is valid and user is authenticated",
                UserId = userId,
                Email = email,
                DisplayName = displayName,
                Roles = roles,
                HighestRole = highestRole,
                ContributionLevel = contributionLevel,
                Claims = allClaims,
                DeveloperEmailAccess = isDeveloperEmail,
                TokenExpiryInfo = GetTokenExpiryInfo(token)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during role check from IP: {ClientIP}", clientIp);
            return Results.Ok(new UserRoleCheckDto
            {
                IsAuthenticated = false,
                Message = $"Error validating token: {ex.Message}",
                Roles = [],
                Email = null,
                HighestRole = "Error",
                Claims = [],
                DeveloperEmailAccess = false
            });
        }
    }

    /// <summary>
    /// Helper method to get token expiry information
    /// </summary>
    private static TokenExpiryInfo? GetTokenExpiryInfo(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return new TokenExpiryInfo
            {
                IssuedAt = jwtToken.ValidFrom,
                ExpiresAt = jwtToken.ValidTo,
                IsExpired = jwtToken.ValidTo < DateTime.UtcNow,
                TimeUntilExpiry = jwtToken.ValidTo - DateTime.UtcNow
            };
        }
        catch
        {
            return null;
        }
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequestDto request,
        [FromServices] ILogger<Program> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        HttpContext context)
    {
        var clientIp = $"{context.Connection.RemoteIpAddress}";

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

            // 🎯 Use async version to get roles properly
            var token = await jwtService.GenerateTokenAsync(user);
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
        var clientIp = $"{context.Connection.RemoteIpAddress}";

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

            // Object initialization with contribution level
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

            // 🎯 Assign default User role to new users
            await userManager.AddToRoleAsync(user, "User");

            // 🎯 Use async version to get roles properly
            var token = await jwtService.GenerateTokenAsync(user);
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
        var clientIp = $"{context.Connection.RemoteIpAddress}";

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

                // 🎯 Generate new tokens with roles
                var newToken = await jwtService.GenerateTokenAsync(user);
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

    // Lambda expression with static modifier
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
        var clientIp = $"{context.Connection.RemoteIpAddress}";

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
        var clientIp = $"{context.Connection.RemoteIpAddress}";

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

/// <summary>
/// C# 13.0 - DTO for user role check response
/// </summary>
public record UserRoleCheckDto
{
    public bool IsAuthenticated { get; init; }
    public string Message { get; init; } = "";
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public List<string> Roles { get; init; } = [];
    public string HighestRole { get; init; } = "";
    public string? ContributionLevel { get; init; }
    public List<ClaimInfo> Claims { get; init; } = [];
    public bool DeveloperEmailAccess { get; init; }
    public TokenExpiryInfo? TokenExpiryInfo { get; init; }
}

/// <summary>
/// C# 13.0 - Claim information for debugging
/// </summary>
public record ClaimInfo
{
    public string Type { get; init; } = "";
    public string Value { get; init; } = "";
}

/// <summary>
/// C# 13.0 - Token expiry information
/// </summary>
public record TokenExpiryInfo
{
    public DateTime IssuedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsExpired { get; init; }
    public TimeSpan TimeUntilExpiry { get; init; }
}