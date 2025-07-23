using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieData.Context;
using Microsoft.AspNetCore.Identity;
using NicolasQuiPaieData.Models;

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
                FiscalLevel = FiscalLevel.PetitNicolas,
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
                FiscalLevel = FiscalLevel.GrosNicolas,
                ReputationScore = 250,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            };

            await userManager.CreateAsync(testUser1, "TestPassword123!");
            await userManager.CreateAsync(testUser2, "TestPassword123!");

            // Create test proposals
            var proposals = new List<Proposal>
            {
                new Proposal
                {
                    Id = 1,
                    Title = "Test Proposal 1",
                    Description = "This is a test proposal description that is long enough to pass validation requirements for our system.",
                    CategoryId = 1,
                    CreatedById = testUser1.Id,
                    Status = ProposalStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    VotesFor = 5,
                    VotesAgainst = 2,
                    ViewsCount = 50,
                    IsFeatured = false
                },
                new Proposal
                {
                    Id = 2,
                    Title = "Test Proposal 2",
                    Description = "This is another test proposal description that is also long enough to pass validation requirements for our testing system.",
                    CategoryId = 2,
                    CreatedById = testUser2.Id,
                    Status = ProposalStatus.Active,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    VotesFor = 12,
                    VotesAgainst = 3,
                    ViewsCount = 120,
                    IsFeatured = true
                },
                new Proposal
                {
                    Id = 3,
                    Title = "Closed Test Proposal",
                    Description = "This is a closed test proposal description that is long enough to pass validation requirements and is used for testing closed proposals.",
                    CategoryId = 1,
                    CreatedById = testUser1.Id,
                    Status = ProposalStatus.Closed,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    ClosedAt = DateTime.UtcNow.AddDays(-1),
                    VotesFor = 25,
                    VotesAgainst = 10,
                    ViewsCount = 300,
                    IsFeatured = false
                }
            };

            context.Proposals.AddRange(proposals);

            // Create test votes
            var votes = new List<Vote>
            {
                new Vote
                {
                    Id = 1,
                    UserId = testUser1.Id,
                    ProposalId = 2,
                    VoteType = VoteType.For,
                    VotedAt = DateTime.UtcNow.AddDays(-3),
                    Weight = 1,
                    Comment = "Great idea!"
                },
                new Vote
                {
                    Id = 2,
                    UserId = testUser2.Id,
                    ProposalId = 1,
                    VoteType = VoteType.Against,
                    VotedAt = DateTime.UtcNow.AddDays(-2),
                    Weight = 2,
                    Comment = "I disagree with this approach"
                }
            };

            context.Votes.AddRange(votes);

            // Create test comments
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    UserId = testUser2.Id,
                    ProposalId = 1,
                    Content = "This is a test comment on the first proposal",
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    LikesCount = 3,
                    IsDeleted = false,
                    IsModerated = false
                },
                new Comment
                {
                    Id = 2,
                    UserId = testUser1.Id,
                    ProposalId = 1,
                    ParentCommentId = 1,
                    Content = "This is a reply to the first comment",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    LikesCount = 1,
                    IsDeleted = false,
                    IsModerated = false
                }
            };

            context.Comments.AddRange(comments);

            await context.SaveChangesAsync();

            logger.LogInformation("Test data seeded successfully");
        }
    }
}