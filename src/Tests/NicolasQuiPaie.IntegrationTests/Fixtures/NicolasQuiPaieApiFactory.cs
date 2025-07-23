using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieAPI.Infrastructure.Data;
using NicolasQuiPaieAPI.Infrastructure.Models;
using ContributionLevel = NicolasQuiPaieAPI.Infrastructure.Models.ContributionLevel;
using ProposalStatus = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus;
using VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace NicolasQuiPaie.IntegrationTests.Fixtures
{
    public class NicolasQuiPaieApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("DisableDbInitialization", "true")
                });
            });

            builder.ConfigureServices(services =>
            {
                // Be more specific about what we remove - only remove EF/SQL Server services, not application services
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>) && 
                     d.ServiceType.GenericTypeArguments.Contains(typeof(ApplicationDbContext))) ||
                    d.ServiceType.FullName?.Contains("Microsoft.EntityFrameworkCore.SqlServer") == true ||
                    d.ImplementationType?.FullName?.Contains("Microsoft.EntityFrameworkCore.SqlServer") == true
                ).ToList();

                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add our in-memory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                    options.EnableSensitiveDataLogging();
                });
            });
        }

        public async Task SeedDatabaseAsync()
        {
            // Don't seed database in the factory initialization, do it lazily when first called
            return;
        }

        public async Task<bool> InitializeTestDataAsync()
        {
            try
            {
                // Create client first to ensure services are initialized
                using var client = CreateClient();
                
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                await context.Database.EnsureCreatedAsync();

                // Clear existing data
                if (context.Votes.Any()) context.RemoveRange(context.Votes);
                if (context.Comments.Any()) context.RemoveRange(context.Comments); 
                if (context.Proposals.Any()) context.RemoveRange(context.Proposals);
                if (context.Categories.Any()) context.RemoveRange(context.Categories);
                if (context.Users.Any()) context.RemoveRange(context.Users);
                await context.SaveChangesAsync();

                // Add test categories
                var categories = new[]
                {
                    new Category { Id = 1, Name = "Test Category 1", Description = "Test Description 1", Color = "#FF0000", IconClass = "fas fa-test", IsActive = true, SortOrder = 1 },
                    new Category { Id = 2, Name = "Test Category 2", Description = "Test Description 2", Color = "#00FF00", IconClass = "fas fa-test2", IsActive = true, SortOrder = 2 }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // Add test user
                var testUser = new ApplicationUser
                {
                    Id = "test-user-1",
                    UserName = "test@test.com",
                    Email = "test@test.com",
                    DisplayName = "Test User",
                    ContributionLevel = ContributionLevel.PetitNicolas,
                    ReputationScore = 100,
                    IsVerified = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(testUser, "Test123!");
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"User creation errors (may be normal): {errors}");
                }

                // Add test proposals
                var proposals = new[]
                {
                    new Proposal
                    {
                        Id = 1,
                        Title = "Test Proposal 1",
                        Description = "This is a test proposal for integration testing that meets the minimum length requirement.",
                        Status = ProposalStatus.Active,
                        CreatedById = testUser.Id,
                        CategoryId = 1,
                        CreatedAt = DateTime.UtcNow,
                        VotesFor = 5,
                        VotesAgainst = 2,
                        ViewsCount = 50
                    }
                };

                context.Proposals.AddRange(proposals);
                await context.SaveChangesAsync();

                // Add test votes to make analytics tests more realistic
                var votes = new[]
                {
                    new Vote
                    {
                        Id = 1,
                        UserId = testUser.Id,
                        ProposalId = 1,
                        VoteType = VoteType.For,
                        VotedAt = DateTime.UtcNow.AddHours(-2)
                    },
                    new Vote
                    {
                        Id = 2,
                        UserId = testUser.Id,
                        ProposalId = 1,
                        VoteType = VoteType.Against,
                        VotedAt = DateTime.UtcNow.AddHours(-1)
                    }
                };

                context.Votes.AddRange(votes);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    using var scope = Services.CreateScope();
                    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    context?.Database.EnsureDeleted();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }
            
            base.Dispose(disposing);
        }
    }
}