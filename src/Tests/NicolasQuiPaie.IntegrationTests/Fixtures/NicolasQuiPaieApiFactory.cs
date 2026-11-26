// The API keeps Serilog's logger in a static: one factory at a time.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace NicolasQuiPaie.IntegrationTests.Fixtures;

/// <summary>
/// The real API, its endpoints, services, repositories and Identity, on a SQLite database of its own (a temporary file:
/// real transactions, foreign keys and unique indexes) seeded with a known set of users, proposals and votes. One instance per test class (class fixture): the tests of a class share its data.
/// </summary>
public class NicolasQuiPaieApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestUserId = "test-user-1";
    public const string TestUserEmail = "test@test.com";
    public const string TestUserPassword = "Test123!";
    public const string OtherUserId = "test-user-2";

    /// <summary>The proposals seeded, ids 1 to 7: "Test Proposal 1" to "Test Proposal 7", all active.</summary>
    public const int SeededProposalCount = 7;

    /// <summary>The mail sender, the one collaborator replaced: the tests read the reset links it is handed.</summary>
    public Mock<IEmailService> Emails { get; } = new();

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"nqp-integration-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DisableDbInitialization", "true");
        builder.UseSetting("DisableHttpsRedirection", "true");

        builder.ConfigureServices(services =>
        {
            // Only the EF Core SQL Server registrations go: every application service stays as the API wires it.
            var descriptorsToRemove = services.Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>) ||
                descriptor.ServiceType.FullName?.Contains("Microsoft.EntityFrameworkCore.SqlServer") == true ||
                descriptor.ImplementationType?.FullName?.Contains("Microsoft.EntityFrameworkCore.SqlServer") == true).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_databasePath}");
                options.ReplaceService<IModelCustomizer, SqliteModelCustomizer>();
                options.EnableSensitiveDataLogging();
            });

            services.RemoveAll<IEmailService>();
            services.AddSingleton(Emails.Object);
        });
    }

    public async ValueTask InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // The categories (ids 1 to 5) and the roles come from HasData, created with the database.
        await context.Database.EnsureCreatedAsync();

        await CreateUserAsync(userManager, TestUserId, TestUserEmail, "Test User", ContributionLevel.PetitNicolas, 100);
        await CreateUserAsync(userManager, OtherUserId, "other@test.com", "Other User", ContributionLevel.GrosNicolas, 300);
        // One user at each of the two other levels: every level has someone in it.
        await CreateUserAsync(userManager, "test-user-3", "middle@test.com", "Middle User", ContributionLevel.GrosMoyenNicolas, 200);
        await CreateUserAsync(userManager, "test-user-4", "supreme@test.com", "Supreme User", ContributionLevel.NicolasSupreme, 1000);

        var proposals = Enumerable.Range(1, SeededProposalCount).Select(id => new Proposal
        {
            Id = id,
            Title = $"Test Proposal {id}",
            Description = $"This is test proposal {id} for integration testing that meets the minimum length requirement.",
            Status = ProposalStatus.Active,
            CreatedById = TestUserId,
            CategoryId = id % 2 == 0 ? 2 : 1,
            CreatedAt = DateTime.UtcNow.AddHours(-id),
            ViewsCount = 10 * id
        }).ToList();
        context.Proposals.AddRange(proposals);

        // One vote of the other user, with the counter of its proposal in step.
        context.Votes.Add(new Vote
        {
            UserId = OtherUserId,
            ProposalId = 1,
            VoteType = VoteType.For,
            Weight = 1,
            VotedAt = DateTime.UtcNow.AddHours(-2)
        });
        proposals[0].VotesFor = 1;

        await context.SaveChangesAsync();
    }

    /// <summary>A client with the bearer token of the given seeded user, made by the API's own JWT service.</summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string userId = TestUserId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await CreateTokenAsync(userId));
        return client;
    }

    public async Task<string> CreateTokenAsync(string userId = TestUserId)
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException($"No seeded user {userId}");
        return await scope.ServiceProvider.GetRequiredService<IJwtService>().GenerateTokenAsync(user);
    }

    /// <summary>Adds a user with the "User" role, as the registration does.</summary>
    public async Task<string> AddUserAsync(string userId, ContributionLevel contributionLevel)
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await CreateUserAsync(userManager, userId, $"{userId}@test.com", $"User {userId}", contributionLevel, 0);
        return userId;
    }

    /// <summary>Reads the database the API writes to, in a scope of its own.</summary>
    public async Task<T> WithContextAsync<T>(Func<ApplicationDbContext, Task<T>> read)
    {
        await using var scope = Services.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
        GC.SuppressFinalize(this);
    }

    private static async Task CreateUserAsync(UserManager<ApplicationUser> userManager, string id, string email, string displayName,
        ContributionLevel contributionLevel, int reputationScore)
    {
        var user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            DisplayName = displayName,
            ContributionLevel = contributionLevel,
            ReputationScore = reputationScore,
            IsVerified = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await userManager.CreateAsync(user, TestUserPassword);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException($"Seeding user {email} failed: {string.Join(", ", created.Errors.Select(error => error.Description))}");
        }

        await userManager.AddToRoleAsync(user, "User");
    }
}
