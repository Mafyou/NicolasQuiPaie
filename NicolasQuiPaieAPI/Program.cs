using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Serilog;
using NicolasQuiPaieAPI.Infrastructure.Data;
using NicolasQuiPaieAPI.Infrastructure.Models;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieAPI.Application.Services;
using NicolasQuiPaieAPI.Infrastructure.Repositories;
using NicolasQuiPaieAPI.Application.Validators;
using NicolasQuiPaieAPI.Application.Mappings;
using NicolasQuiPaieAPI.Presentation.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nicolas-qui-paie-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Nicolas Qui Paie API", 
        Version = "v1",
        Description = "API pour la plateforme de démocratie participative Nicolas Qui Paie"
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
                "https://localhost:7051"  // API HTTPS
              )
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
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nicolas Qui Paie API v1");
        c.RoutePrefix = string.Empty; // Swagger UI à la racine
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

// Mappage des endpoints
app.MapAuthenticationEndpoints();
app.MapProposalEndpoints();
app.MapVotingEndpoints();
app.MapCategoryEndpoints();
app.MapCommentEndpoints();
app.MapAnalyticsEndpoints();

// Endpoint de santé
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
   .WithTags("Health")
   .WithSummary("Vérification de l'état de l'API");

// Default root endpoint and swagger redirects
app.MapGet("/", () => Results.Content("""
<!DOCTYPE html>
<html>
<head>
    <title>Nicolas Qui Paie API</title>
    <meta http-equiv="refresh" content="0; url=/">
</head>
<body>
    <h1>Welcome to Nicolas Qui Paie API</h1>
    <p>Redirecting to Swagger documentation...</p>
    <p>If you are not redirected automatically, <a href="/">click here</a>.</p>
</body>
</html>
""", "text/html"))
   .WithTags("Root")
   .WithSummary("API Documentation Homepage");

// Handle common swagger URL variations
app.MapGet("/swagger", () => Results.Redirect("/"))
   .WithTags("Root")
   .WithSummary("Redirect Swagger to root");

app.MapGet("/Swagger", () => Results.Redirect("/"))
   .WithTags("Root")
   .WithSummary("Redirect Swagger (capital S) to root");

// Initialisation de la base de données
await using var scope = app.Services.CreateAsyncScope(); // C# 13.0 - await using improvement
var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

try
{
    await context.Database.EnsureCreatedAsync(); // C# 13.0 - Use async version
    
    // Seed categories first
    await SeedCategoriesAsync(context, logger);
    
    // Seed test user for development
    await SeedTestUserAsync(userManager, logger);
    
    logger.LogInformation("Base de données initialisée avec succès");
}
catch (Exception ex)
{
    logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
}

await app.RunAsync(); // C# 13.0 - Use async version

// Seed categories method
static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger<Program> logger)
{
    try
    {
        if (!await context.Categories.AnyAsync()) // C# 13.0 - Use async version
        {
            Category[] categories = [ // C# 13.0 - Collection expressions
                new()
                {
                    Name = "Fiscalité",
                    Description = "Propositions concernant les impôts et taxes",
                    Color = "#FF6B6B",
                    IconClass = "fas fa-coins",
                    IsActive = true
                },
                new()
                {
                    Name = "Dépenses Publiques",
                    Description = "Propositions sur l'utilisation des deniers publics",
                    Color = "#4ECDC4",
                    IconClass = "fas fa-hand-holding-usd",
                    IsActive = true
                },
                new()
                {
                    Name = "Social",
                    Description = "Propositions sociales et de solidarité",
                    Color = "#45B7D1",
                    IconClass = "fas fa-users",
                    IsActive = true
                },
                new()
                {
                    Name = "Économie",
                    Description = "Propositions économiques et de relance",
                    Color = "#FFA07A",
                    IconClass = "fas fa-chart-line",
                    IsActive = true
                },
                new()
                {
                    Name = "Environnement",
                    Description = "Propositions écologiques et environnementales",
                    Color = "#98D8C8",
                    IconClass = "fas fa-leaf",
                    IsActive = true
                }
            ];

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("Créé {Count} catégories par défaut", categories.Length);
        }
        else
        {
            logger.LogInformation("Catégories déjà présentes en base");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la création des catégories par défaut");
    }
}

// Seed test user method
static async Task SeedTestUserAsync(UserManager<ApplicationUser> userManager, ILogger<Program> logger)
{
    try
    {
        const string testEmail = "nicolas@test.fr";
        const string testPassword = "Test123!";
        
        var existingUser = await userManager.FindByEmailAsync(testEmail);
        
        if (existingUser == null)
        {
            var testUser = new ApplicationUser
            {
                UserName = testEmail,
                Email = testEmail,
                EmailConfirmed = true,
                DisplayName = "Nicolas Test API",
                FiscalLevel = NicolasQuiPaieAPI.Infrastructure.Models.FiscalLevel.PetitNicolas,
                CreatedAt = DateTime.UtcNow,
                ReputationScore = 0,
                IsVerified = true,
                Bio = "Utilisateur de test pour l'API Nicolas Qui Paie"
            };

            var result = await userManager.CreateAsync(testUser, testPassword);
            
            if (result.Succeeded)
            {
                logger.LogInformation("Utilisateur de test créé avec succès: {Email}", testEmail);
            }
            else
            {
                logger.LogWarning("Échec de création de l'utilisateur de test: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Utilisateur de test existe déjà: {Email}", testEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la création de l'utilisateur de test");
    }
}
