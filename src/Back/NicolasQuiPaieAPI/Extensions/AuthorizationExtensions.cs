namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Extension methods for authorization configuration with modern language features
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Configure role-based authorization policies
    /// </summary>
    public static IServiceCollection AddNicolasQuiPaieAuthorization(this IServiceCollection services)
    {
        // Add Identity with roles support
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure authorization policies with modern syntax
        services.AddAuthorization(options =>
        {
            // Basic authenticated user policy
            options.AddPolicy(NicolasPolicies.User, policy =>
                policy.RequireRole([NicolasRoles.User, NicolasRoles.SuperUser, NicolasRoles.Admin, NicolasRoles.Developer]));

            // SuperUser policy - can manage proposals
            options.AddPolicy(NicolasPolicies.SuperUser, policy =>
                policy.RequireRole([NicolasRoles.SuperUser, NicolasRoles.Admin]));

            // Admin policy - admin access
            options.AddPolicy(NicolasPolicies.Admin, policy =>
                policy.RequireRole(NicolasRoles.Admin));

            // Developer policy - developer access
            options.AddPolicy(NicolasPolicies.Developer, policy =>
                policy.RequireRole(NicolasRoles.Developer));

            // Contribution level policies
            options.AddPolicy(NicolasPolicies.HighContributor, policy =>
                policy.RequireAssertion(context =>
                {
                    var levelClaim = context.User.FindFirst("ContributionLevel")?.Value;
                    return Enum.TryParse<Infrastructure.Models.ContributionLevel>(levelClaim, out var level) &&
                           level is Infrastructure.Models.ContributionLevel.GrosNicolas or Infrastructure.Models.ContributionLevel.NicolasSupreme;
                }));

            // Verified user policy
            options.AddPolicy(NicolasPolicies.Verified, policy =>
                policy.RequireAssertion(context =>
                {
                    var isVerifiedClaim = context.User.FindFirst("IsVerified")?.Value;
                    return bool.TryParse(isVerifiedClaim, out var isVerified) && isVerified;
                }));
        });

        return services;
    }

    /// <summary>
    /// Configure JWT Bearer authentication with role support
    /// </summary>
    public static IServiceCollection AddNicolasQuiPaieJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "MySecretKeyForNicolasQuiPaie2025!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "NicolasQuiPaieAPI";
        var jwtAudience = configuration["Jwt:Audience"] ?? "NicolasQuiPaieClient";

        // Prevent default JWT role claim mapping
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                RoleClaimType = ClaimTypes.Role // Enable role claims
            };
        });

        return services;
    }

    /// <summary>
    /// Require User role or higher
    /// </summary>
    public static TBuilder RequireUserRole<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.User);

    /// <summary>
    /// Require SuperUser role or higher
    /// </summary>
    public static TBuilder RequireSuperUserRole<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.SuperUser);

    /// <summary>
    /// Require Admin role
    /// </summary>
    public static TBuilder RequireAdminRole<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.Admin);

    /// <summary>
    /// Require Developer access
    /// </summary>
    public static TBuilder RequireDeveloperAccess<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.Developer);

    /// <summary>
    /// Require verified user
    /// </summary>
    public static TBuilder RequireVerifiedUser<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.Verified);

    /// <summary>
    /// Require high contributor status
    /// </summary>
    public static TBuilder RequireHighContributor<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(NicolasPolicies.HighContributor);
}