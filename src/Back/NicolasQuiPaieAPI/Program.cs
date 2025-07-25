using NicolasQuiPaieAPI.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configure Serilog with custom ApiLog table mapping
builder.AddCustomSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();

// Configuration Swagger - ONLY in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Nicolas Qui Paie API",
            Version = "v1",
            Description = "API pour la plateforme de démocratie participative Nicolas Qui Paie - Mode Développement"
        });

        // Configuration JWT pour Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });
    });
}

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7084", // NicolasQuiPaieWeb HTTPS
                "http://localhost:5207",  // NicolasQuiPaieWeb HTTP
                "https://localhost:5001",
                "http://localhost:5000",
                "https://localhost:7398", // API dans appsettings
                "https://localhost:7051",  // API HTTPS
                "https://happy-ocean-06624de03.2.azurestaticapps.net", // NicolasQuiPaieWeb Azure Static Web App
                "https://*.azurestaticapps.net"
              )
              .SetIsOriginAllowedToAllowWildcardSubdomains() // Allow subdomains
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// In development, also allow any localhost
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevelopmentCors", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;

                // Allow any localhost origin in development
                return origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
}

// Configuration Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuration Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuration JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MySecretKeyForNicolasQuiPaie2024!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NicolasQuiPaieAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "NicolasQuiPaieClient";

builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// Configuration AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configuration FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateProposalDtoValidator>();

// Injection des dépendances - Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApiLogRepository, ApiLogRepository>();

// Injection des dépendances - Services
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IVotingService, VotingService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Swagger ONLY available in Development mode
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nicolas Qui Paie API v1 - Development");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
        c.DocumentTitle = "Nicolas Qui Paie API - Development Mode";
    });

    // Use development CORS in development
    app.UseCors("DevelopmentCors");
}
else
{
    // Use restrictive CORS in production
    app.UseCors("AllowBlazorClient");
}

// Only use HTTPS redirection if not explicitly disabled
var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// 🔧 Add custom Serilog request logging middleware
app.UseCustomSerilogRequestLogging();

// 🎯 Map all API endpoints using extension method
app.MapApiEndpoints();

// Default root endpoint and swagger redirects
app.MapGet("/", () =>
{
    if (app.Environment.IsDevelopment())
    {
        return Results.Content("""
        <!DOCTYPE html>
        <html>
        <head>
            <title>Nicolas Qui Paie API - Development</title>
            <meta http-equiv="refresh" content="0; url=/">
        </head>
        <body>
            <h1>Welcome to Nicolas Qui Paie API - Development Mode</h1>
            <p>Redirecting to Swagger documentation...</p>
            <p>If you are not redirected automatically, <a href="/">click here</a>.</p>
            <hr>
            <p><strong>Available endpoints:</strong></p>
            <ul>
                <li><a href="/swagger">Swagger Documentation</a></li>
                <li><a href="/health">Health Check</a></li>
                <li><a href="/api/logs">API Logs</a></li>
                <li><a href="/swagger/index.html">Test Logging (POST)</a></li>
            </ul>
            <p><em>Logging Level: Warning, Error, Fatal only</em></p>
            <p><em>SQL Logging: Enabled with custom ApiLog table mapping</em></p>
        </body>
        </html>
        """, "text/html");
    }
    else
    {
        return Results.Ok(new
        {
            Status = "Nicolas Qui Paie API",
            Environment = "Production",
            Timestamp = DateTime.UtcNow,
            Message = "API is running. Swagger documentation is only available in development mode.",
            LoggingLevel = "Warning, Error, Fatal only",
            SqlLoggingEnabled = true
        });
    }
})
.WithTags("Root")
.WithSummary("API root endpoint");

// Handle swagger redirects only in development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/swagger", () => Results.Redirect("/"))
       .WithTags("Root")
       .WithSummary("Redirect Swagger to root");

    app.MapGet("/Swagger", () => Results.Redirect("/"))
       .WithTags("Root")
       .WithSummary("Redirect Swagger (capital S) to root");
}

// 🎯 Initialize database using extension method
await app.InitializeDatabaseAsync();

Log.Information("🚀 Nicolas Qui Paie API started in {Environment} mode. Swagger: {SwaggerEnabled}. SQL Logging: Enabled with custom ApiLog table", 
    app.Environment.EnvironmentName, app.Environment.IsDevelopment() ? "Available" : "Disabled");

await app.RunAsync(); // C# 13.0 - Use async version

// Make Program class accessible for testing
public partial class Program { }
