namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Extension methods for application configuration and database initialization
/// </summary>
public static class SeedExtensions
{
    /// <summary>
    /// Initializes the database with seeded data
    /// </summary>
    public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
    {
        var disableDbInitialization = app.Configuration.GetValue<bool>("DisableDbInitialization");
        if (disableDbInitialization) return app;

        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            // Seed categories first
            await SeedCategoriesAsync(context, logger);

            // Seed test user for development
            await SeedTestUserAsync(userManager, logger);

            // Test logging after DB initialization
            logger.LogWarning("Database initialization completed - SQL logging test with custom ApiLog table");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error during database initialization in {Environment}", app.Environment.EnvironmentName);
            throw;
        }

        return app;
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context, Microsoft.Extensions.Logging.ILogger logger)
    {
        try
        {
            if (!await context.Categories.AnyAsync())
            {
                Category[] categories = [
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
                Log.Warning("Created {Count} default categories", categories.Length);
            }
            else
            {
                logger.LogWarning("Categories already exist in database - skipping seeding");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating default categories");
            throw;
        }
    }

    private static async Task SeedTestUserAsync(UserManager<ApplicationUser> userManager, Microsoft.Extensions.Logging.ILogger logger)
    {
        try
        {
            const string testEmail = "nicolas@test.fr";
            const string testPassword = "Test123!";

            var existingUser = await userManager.FindByEmailAsync(testEmail);

            if (existingUser is null)
            {
                var testUser = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    EmailConfirmed = true,
                    DisplayName = "Nicolas Test API",
                    ContributionLevel = NicolasQuiPaieAPI.Infrastructure.Models.ContributionLevel.PetitNicolas,
                    CreatedAt = DateTime.UtcNow,
                    ReputationScore = 0,
                    IsVerified = true,
                    Bio = "Utilisateur de test pour l'API Nicolas Qui Paie - Niveau de contribution initial"
                };

                var result = await userManager.CreateAsync(testUser, testPassword);

                if (result.Succeeded)
                {
                    Log.Warning("Test user created successfully: {Email} - Contribution Level: {Level}",
                        testEmail, testUser.ContributionLevel);
                }
                else
                {
                    logger.LogWarning("Failed to create test user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogWarning("Test user already exists: {Email} - skipping creation", testEmail);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating test user");
            throw;
        }
    }
}