namespace NicolasQuiPaieData.DTOs;

/// <summary>
/// DTO for user login request (class with setters for two-way binding)
/// </summary>
public class LoginRequestDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; } = false;
}

/// <summary>
/// DTO for user registration request (class with setters for two-way binding)
/// </summary>
public class RegisterRequestDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? Bio { get; set; }
}

/// <summary>
/// DTO for authentication response (record for immutability during transfer)
/// </summary>
public record AuthResponseDto
{
    public bool Success { get; init; }
    public string Token { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = new();
    public IReadOnlyList<string> Errors { get; init; } = [];
}

/// <summary>
/// DTO for token refresh request (record for immutability)
/// </summary>
public record RefreshTokenRequestDto
{
    public string Token { get; init; } = "";
    public string RefreshToken { get; init; } = "";
    public string UserId { get; init; } = "";
}

/// <summary>
/// DTO for password reset request (record for immutability)
/// </summary>
public record PasswordResetRequestDto
{
    public string Email { get; init; } = "";
}

/// <summary>
/// DTO for password reset confirmation (record for immutability)
/// </summary>
public record PasswordResetConfirmDto
{
    public string Email { get; init; } = "";
    public string Token { get; init; } = "";
    public string NewPassword { get; init; } = "";
}

/// <summary>
/// Static constants for role names using modern constant patterns
/// </summary>
public static class NicolasRoles
{
    public const string User = "User";
    public const string SuperUser = "SuperUser";
    public const string Admin = "Admin";
    public const string Developer = "Developer";

    /// <summary>
    /// Collection expression for all roles
    /// </summary>
    public static readonly string[] All = [User, SuperUser, Admin, Developer];

    /// <summary>
    /// Collection expression for elevated roles
    /// </summary>
    public static readonly string[] Elevated = [SuperUser, Admin, Developer];
}

/// <summary>
/// Static constants for policy names using modern constant patterns
/// </summary>
public static class NicolasPolicies
{
    public const string User = "UserPolicy";
    public const string SuperUser = "SuperUserPolicy";
    public const string Admin = "AdminPolicy";
    public const string Developer = "DeveloperPolicy";
    public const string HighContributor = "HighContributorPolicy";
    public const string Verified = "VerifiedPolicy";
}