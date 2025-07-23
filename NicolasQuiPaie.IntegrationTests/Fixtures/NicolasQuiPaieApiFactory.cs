using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieAPI.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using NicolasQuiPaieAPI.Infrastructure.Models;
using ContributionLevel = NicolasQuiPaieAPI.Infrastructure.Models.ContributionLevel;
using ProposalStatus = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus;

namespace NicolasQuiPaie.IntegrationTests.Fixtures
{
    public class NicolasQuiPaieApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid().ToString());
                });

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices.GetRequiredService<ApplicationDbContext>();
                var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                var logger = scopedServices.GetRequiredService<ILogger<NicolasQuiPaieApiFactory>>();

                // Ensure the database is created
                context.Database.EnsureCreated();

                try
                {
                    // Seed the database with test data
                    SeedTestData(context, userManager, logger).Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the database with test data");
                }
            });

            builder.UseEnvironment("Testing");
        }

        private static async Task SeedTestData(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
        {
            // Clear existing data
            context.Proposals.RemoveRange(context.Proposals);
            context.Votes.RemoveRange(context.Votes);
            context.Comments.RemoveRange(context.Comments);
            await context.SaveChangesAsync();

            // Create test users
            var testUser1 = new ApplicationUser
            {
                Id = "test-user-1",
                UserName = "testuser1@test.com",
                Email = "testuser1@test.com",
                EmailConfirmed = true,
                DisplayName = "Test User 1",
                ContributionLevel = ContributionLevel.PetitNicolas,
                ReputationScore = 100,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var testUser2 = new ApplicationUser
            {
                Id = "test-user-2",
                UserName = "testuser2@test.com",
                Email = "testuser2@test.com",
                EmailConfirmed = true,
                DisplayName = "Test User 2",
                ContributionLevel = ContributionLevel.GrosMoyenNicolas,
                ReputationScore = 250,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-45)
            };

            // Add users using UserManager
            await userManager.CreateAsync(testUser1);
            await userManager.CreateAsync(testUser2);

            // Create test categories
            var categories = new[]
            {
                new Category { Id = 1, Name = "Test Category 1", Description = "Test Description 1", Color = "#FF0000", IconClass = "fas fa-test" },
                new Category { Id = 2, Name = "Test Category 2", Description = "Test Description 2", Color = "#00FF00", IconClass = "fas fa-test2" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Create test proposals
            var proposals = new[]
            {
                new Proposal
                {
                    Id = 1,
                    Title = "Test Proposal 1",
                    Description = "This is a test proposal description for testing purposes.",
                    Status = ProposalStatus.Active,
                    CreatedById = testUser1.Id,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    VotesFor = 10,
                    VotesAgainst = 5,
                    ViewsCount = 100
                },
                new Proposal
                {
                    Id = 2,
                    Title = "Test Proposal 2",
                    Description = "This is another test proposal description for testing purposes.",
                    Status = ProposalStatus.Active,
                    CreatedById = testUser2.Id,
                    CategoryId = 2,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    VotesFor = 15,
                    VotesAgainst = 3,
                    ViewsCount = 150
                }
            };

            context.Proposals.AddRange(proposals);
            await context.SaveChangesAsync();

            logger.LogInformation("Test data seeded successfully");
        }
    }
}