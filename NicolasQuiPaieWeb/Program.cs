using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieWeb.Components;
using NicolasQuiPaieWeb.Data;
using NicolasQuiPaieWeb.Data.Models;
using NicolasQuiPaieWeb.Services;
using NicolasQuiPaieWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Razor Pages support for Identity
builder.Services.AddRazorPages();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add DbContextFactory for resolving DbContext threading issues in Blazor components
// Use a separate registration to avoid the scoped/singleton conflict
builder.Services.AddSingleton<IDbContextFactory<ApplicationDbContext>>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    optionsBuilder.UseSqlServer(connectionString);
    
    return new CustomDbContextFactory<ApplicationDbContext>(optionsBuilder.Options);
});

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add SignalR
builder.Services.AddSignalR();

// Add custom services
builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<VotingService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<BadgeService>(); // Service d'évolution des badges

// Add email sender (no-op for demo)
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, NoOpEmailSender>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapRazorPages();
app.MapHub<VotingHub>("/votingHub");

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        context.Database.EnsureCreated();
        
        // Seed test user for development
        await SeedTestUserAsync(userManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
    }
}

app.Run();

// Seed test user method
static async Task SeedTestUserAsync(UserManager<ApplicationUser> userManager, ILogger logger)
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
                DisplayName = "Nicolas Test",
                FiscalLevel = FiscalLevel.PetitNicolas,
                CreatedAt = DateTime.UtcNow,
                ReputationScore = 0,
                IsVerified = true,
                Bio = "Utilisateur de test pour la plateforme Nicolas Qui Paie"
            };

            var result = await userManager.CreateAsync(testUser, testPassword);
            
            if (result.Succeeded)
            {
                logger.LogInformation($"Utilisateur de test créé avec succès: {testEmail}");
            }
            else
            {
                logger.LogWarning($"Échec de création de l'utilisateur de test: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            logger.LogInformation($"Utilisateur de test existe déjà: {testEmail}");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la création de l'utilisateur de test");
    }
}

// Custom DbContextFactory implementation to avoid DI conflicts
public class CustomDbContextFactory<TContext> : IDbContextFactory<TContext> where TContext : DbContext
{
    private readonly DbContextOptions<TContext> _options;

    public CustomDbContextFactory(DbContextOptions<TContext> options)
    {
        _options = options;
    }

    public TContext CreateDbContext()
    {
        return (TContext)Activator.CreateInstance(typeof(TContext), _options)!;
    }
}

// No-op email sender for demo purposes
public class NoOpEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // For demo purposes, we'll just log the email
        Console.WriteLine($"Email to {email}: {subject}");
        return Task.CompletedTask;
    }
}
