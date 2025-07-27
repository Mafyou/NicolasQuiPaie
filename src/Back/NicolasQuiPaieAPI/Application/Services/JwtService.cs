namespace NicolasQuiPaieAPI.Application.Services;

/// <summary>
/// Enhanced JWT service with role support and modern language features
/// </summary>
public class JwtService(IConfiguration configuration, UserManager<ApplicationUser> userManager) : IJwtService
{
    private readonly string _key = configuration["Jwt:Key"] ?? "MySecretKeyForNicolasQuiPaie2025!";
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "NicolasQuiPaieAPI";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "NicolasQuiPaieClient";
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    /// <summary>
    /// Generate JWT token with roles using modern async patterns
    /// </summary>
    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);

        // Get user roles asynchronously
        var roles = await _userManager.GetRolesAsync(user);

        // Collection expression for claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("DisplayName", user.DisplayName ?? ""),
            new("ContributionLevel", $"{user.ContributionLevel}"),
            new("ReputationScore", $"{user.ReputationScore}"),
            new("IsVerified", $"{user.IsVerified}"),
            new("CreatedAt", user.CreatedAt.ToString("O"))
        };

        // Add role claims using modern syntax
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Modern null-conditional assignment
        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryInMinutes"], out var minutes) ? minutes : 60;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Backward compatibility method with default role assignment
    /// </summary>
    public string GenerateToken(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);

        // Collection expression for basic claims
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("DisplayName", user.DisplayName ?? ""),
            new("ContributionLevel", $"{user.ContributionLevel}"),
            new("ReputationScore", $"{user.ReputationScore}"),
            new("IsVerified", $"{user.IsVerified}"),
            new("CreatedAt", user.CreatedAt.ToString("O")),
            // Default role for backward compatibility
            new(ClaimTypes.Role, NicolasRoles.User)
        };

        var expiryMinutes = int.TryParse(configuration["Jwt:ExpiryInMinutes"], out var minutes) ? minutes : 60;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generate refresh token using modern cryptographic methods
    /// </summary>
    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    /// <summary>
    /// Validate expired token with role support
    /// </summary>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = false, // Don't validate lifetime for expired tokens
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role // Enable role claims
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validate token with role support
    /// </summary>
    public bool IsTokenValid(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role // Enable role claims
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validate token and return principal with role support
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                // RoleClaimType = ClaimTypes.Role // Enable role claims
                RoleClaimType = "role" // Enable role
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extract user roles from token using modern pattern matching
    /// </summary>
    public IEnumerable<string> GetUserRoles(string token)
    {
        var principal = ValidateToken(token);

        // Modern null checking and LINQ
        var rolesClaims = principal?.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? [];
        var roles = principal?.FindAll("role")?.Select(c => c.Value) ?? [];
        return rolesClaims.Concat(roles).Distinct(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if user has specific role using modern pattern matching
    /// </summary>
    public bool HasRole(string token, string role)
    {
        var roles = GetUserRoles(token);
        return roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if user has any of the specified roles
    /// </summary>
    public bool HasAnyRole(string token, params string[] roles)
    {
        var userRoles = GetUserRoles(token);
        return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get highest role using modern pattern matching
    /// </summary>
    public string GetHighestRole(string token)
    {
        var userRoles = GetUserRoles(token).ToList();

        // Pattern matching for role hierarchy
        return userRoles switch
        {
            var roles when roles.Contains(NicolasRoles.Developer, StringComparer.OrdinalIgnoreCase) => NicolasRoles.Developer,
            var roles when roles.Contains(NicolasRoles.Admin, StringComparer.OrdinalIgnoreCase) => NicolasRoles.Admin,
            var roles when roles.Contains(NicolasRoles.SuperUser, StringComparer.OrdinalIgnoreCase) => NicolasRoles.SuperUser,
            var roles when roles.Contains(NicolasRoles.User, StringComparer.OrdinalIgnoreCase) => NicolasRoles.User,
            _ => "Guest"
        };
    }
}