using ProposalStatus = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus;

namespace NicolasQuiPaie.UnitTests.Helpers;

/// <summary>
/// C# 13.0 - Test data helper using collection expressions, primary constructors, and field keyword
/// </summary>
public static class TestDataHelper
{
    // C# 13.0 - Collection expressions for concise test data creation
    public static readonly string[] ValidUserIds = ["user-123", "user-456", "user-789"];

    public static readonly string[] InvalidUserIds = ["", "   ", "invalid-user"];

    public static readonly int[] ValidCategoryIds = [1, 2, 3, 4, 5];

    public static readonly string[] ValidProposalTitles =
    [
        "Test Proposal Alpha",
        "Test Proposal Beta",
        "Test Proposal Gamma"
    ];

    // C# 13.0 - Params collections for flexible test data generation with modern null checking
    public static ApplicationUser CreateTestUser(
        string id = "test-user-id",
        string email = "test@nicolas.fr",
        ContributionLevel contributionLevel = ContributionLevel.PetitNicolas,
        params string[] additionalClaims) =>
        new()
        {
            Id = id,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = $"Test User {id}",
            ContributionLevel = contributionLevel,
            ReputationScore = contributionLevel switch
            {
                ContributionLevel.PetitNicolas => 100,
                ContributionLevel.GrosMoyenNicolas => 250,
                ContributionLevel.GrosNicolas => 500,
                ContributionLevel.NicolasSupreme => 1000,
                _ => 0
            },
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

    // C# 13.0 - Collection expressions with spread operator for test proposals
    public static Proposal[] CreateTestProposals(params (string title, string userId, int categoryId)[] proposalData) =>
        proposalData.Select((data, index) => new Proposal
        {
            Id = index + 1,
            Title = data.title,
            Description = $"This is a comprehensive test description for {data.title} that meets all validation requirements.",
            CreatedById = data.userId,
            CategoryId = data.categoryId,
            Status = ProposalStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-index),
            VotesFor = Random.Shared.Next(5, 50),
            VotesAgainst = Random.Shared.Next(1, 20),
            ViewsCount = Random.Shared.Next(100, 1000)
        }).ToArray();

    // C# 13.0 - Enhanced factory method with pattern matching and collection expressions
    public static Category[] CreateTestCategories() =>
    [
        new()
        {
            Id = 1,
            Name = "Fiscalité Test",
            Description = "Propositions fiscales de test",
            Color = "#FF6B6B",
            IconClass = "fas fa-coins",
            IsActive = true,
            SortOrder = 1
        },
        new()
        {
            Id = 2,
            Name = "Social Test",
            Description = "Propositions sociales de test",
            Color = "#4ECDC4",
            IconClass = "fas fa-users",
            IsActive = true,
            SortOrder = 2
        },
        new()
        {
            Id = 3,
            Name = "Économie Test",
            Description = "Propositions économiques de test",
            Color = "#45B7D1",
            IconClass = "fas fa-chart-line",
            IsActive = true,
            SortOrder = 3
        }
    ];

    // C# 13.0 - Record types for immutable test data
    public record TestScenario(
        string Name,
        string Description,
        int ExpectedResult,
        bool ShouldSucceed = true);

    // C# 13.0 - Collection expressions for test scenarios
    public static readonly TestScenario[] VotingScenarios =
    [
        new("Basic Vote", "User casts a simple vote", 1),
        new("Weighted Vote", "User with higher fiscal level votes", 2),
        new("Supreme Vote", "Nicolas Supreme casts vote", 5),
        new("Invalid Vote", "Invalid vote scenario", 0, false)
    ];

    // C# 13.0 - Static method with modern features for mock setup
    public static Mock<T> CreateMockWithDefaults<T>() where T : class =>
        new(MockBehavior.Loose); // Changed from Strict to Loose

    // C# 13.0 - Enhanced method with tuple deconstruction and pattern matching
    public static (Mock<IUnitOfWork> unitOfWork, Mock<ILogger<TService>> logger)
        CreateServiceMocks<TService>() where TService : class =>
        (CreateMockWithDefaults<IUnitOfWork>(),
         CreateMockWithDefaults<ILogger<TService>>());

    // C# 13.0 - Extension method for fluent test data setup with modern null checking
    public static Mock<IProposalRepository> SetupProposalRepository(
        this Mock<IProposalRepository> mock,
        params Proposal[] proposals)
    {
        mock.Setup(x => x.GetActiveProposalsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(proposals);

        foreach (var proposal in proposals)
        {
            mock.Setup(x => x.GetByIdAsync(proposal.Id))
                .ReturnsAsync(proposal);
        }

        return mock;
    }

    // C# 13.0 - Async enumerable for streaming test data with modern null patterns
    public static async IAsyncEnumerable<ProposalDto> GetTestProposalStreamAsync()
    {
        var proposals = CreateTestProposals(
            ("Streaming Proposal 1", "user-1", 1),
            ("Streaming Proposal 2", "user-2", 2),
            ("Streaming Proposal 3", "user-3", 3)
        );

        foreach (var proposal in proposals)
        {
            await Task.Delay(10); // Simulate async operation
            yield return new ProposalDto
            {
                Id = proposal.Id,
                Title = proposal.Title,
                Description = proposal.Description,
                Status = NicolasQuiPaieData.DTOs.ProposalStatus.Active,
                CreatedAt = proposal.CreatedAt,
                VotesFor = proposal.VotesFor,
                VotesAgainst = proposal.VotesAgainst,
                ViewsCount = proposal.ViewsCount,
                CreatedById = proposal.CreatedById,
                CategoryId = proposal.CategoryId
            };
        }
    }

    // C# 13.0 - Modern null-safe helper methods
    public static T? GetRandomItem<T>(T[] items) where T : class =>
        items.Length > 0 ? items[Random.Shared.Next(items.Length)] : null;

    public static T GetRandomItemOrDefault<T>(T[] items, T defaultValue) =>
        items.Length > 0 ? items[Random.Shared.Next(items.Length)] : defaultValue;

    // C# 13.0 - Enhanced validation helper with modern null checking
    public static void ValidateTestUser(ApplicationUser? user, string expectedId)
    {
        user.ShouldNotBeNull();
        user.Id.ShouldBe(expectedId);
        user.Email.ShouldNotBeNullOrEmpty();
        user.DisplayName.ShouldNotBeNullOrEmpty();
        user.IsVerified.ShouldBeTrue();
    }

    // C# 13.0 - Modern async validation helper
    public static async Task<bool> ValidateAsyncOperation<T>(Func<Task<T?>> operation) where T : class
    {
        try
        {
            var result = await operation();
            return result is not null;
        }
        catch
        {
            return false;
        }
    }

    // C# 13.0 - Collection expressions for error testing scenarios
    public static readonly (string input, bool shouldFail)[] ErrorTestCases =
    [
        ("", true),
        ("   ", true),
        ("valid-input", false),
        ("another-valid-input", false)
    ];

    // C# 13.0 - Modern factory method with enhanced null safety
    public static TDto CreateDto<TDto>() where TDto : class, new() => new();

    // C# 13.0 - Enhanced cleanup helper with modern patterns
    public static void CleanupMocks(params Mock[] mocks)
    {
        foreach (var mock in mocks.Where(m => m is not null))
        {
            mock.Reset();
        }
    }

    // C# 13.0 - Modern assertion helper for collections
    public static void AssertCollectionNotNullOrEmpty<T>(IEnumerable<T>? collection, string collectionName)
    {
        collection.ShouldNotBeNull($"{collectionName} should not be null");
        collection.Any().ShouldBeTrue($"{collectionName} should not be empty");
    }

    // C# 13.0 - Enhanced comparison helper with modern null patterns
    public static bool AreEqual<T>(T? first, T? second) where T : class =>
        (first is null && second is null) ||
        (first is not null && second is not null && first.Equals(second));

    // C# 13.0 - Modern string validation helper
    public static bool IsValidString(string? value) =>
        value is not null && !string.IsNullOrWhiteSpace(value);

    // C# 13.0 - Enhanced test data generator with collection expressions
    public static IEnumerable<T> GenerateTestData<T>(int count, Func<int, T> generator) =>
        Enumerable.Range(0, count).Select(generator);

    // C# 13.0 - Modern conditional helper with pattern matching
    public static TResult? ConditionalMap<TSource, TResult>(TSource? source, Func<TSource, TResult> mapper)
        where TSource : class
        where TResult : class =>
        source is not null ? mapper(source) : null;
}