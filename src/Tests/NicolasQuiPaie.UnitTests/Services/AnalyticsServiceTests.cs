namespace NicolasQuiPaie.UnitTests.Services;

[TestFixture]
public class AnalyticsServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<ILogger<AnalyticsService>> _mockLogger = null!;
    private AnalyticsService _analyticsService = null!;

    [SetUp]
    public void SetUp()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<AnalyticsService>>();

        _analyticsService = new AnalyticsService(_context, _mockLogger.Object);

        // Seed test data
        SeedTestData();
    }

    [Test]
    public async Task GetGlobalStatsAsync_ShouldReturnCompleteStats()
    {
        // Act
        var result = await _analyticsService.GetGlobalStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.TotalUsers.ShouldBe(5);
        result.TotalProposals.ShouldBe(3);
        result.TotalVotes.ShouldBe(6);
        result.TotalComments.ShouldBe(4);
        result.ActiveProposals.ShouldBe(2);
        result.AverageParticipationRate.ShouldBe(2.0); // (6 + 4) / 5
    }

    [Test]
    public async Task GetDashboardStatsAsync_ShouldReturnDashboardStats()
    {
        // Act
        var result = await _analyticsService.GetDashboardStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.TotalUsers.ShouldBe(5);
        result.ActiveUsers.ShouldBe(3); // Users with recent login
        result.TotalProposals.ShouldBe(3);
        result.ActiveProposals.ShouldBe(2);
        result.TotalVotes.ShouldBe(6);
        result.TotalComments.ShouldBe(4);
        result.RasLebolMeter.ShouldBe(33.33, 0.01); // 2/6 * 100 = 33.33%
    }

    [Test]
    public async Task GetContributionLevelDistributionAsync_ShouldReturnCorrectDistribution()
    {
        // Act
        var result = await _analyticsService.GetContributionLevelDistributionAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Distribution.ShouldNotBeNull();
        result.Distribution.Count.ShouldBe(4);

        var petitNicolas = result.Distribution.FirstOrDefault(d => d.LevelName == "PetitNicolas");
        petitNicolas.ShouldNotBeNull();
        petitNicolas.UserCount.ShouldBe(2);
        petitNicolas.Percentage.ShouldBe(40.0);
    }

    [Test]
    public async Task GetTopContributorsAsync_ShouldReturnTopContributors()
    {
        // Act
        var result = await _analyticsService.GetTopContributorsAsync(10);

        // Assert
        result.ShouldNotBeNull();
        result.TopProposers.ShouldNotBeNull();
        result.TopVoters.ShouldNotBeNull();
        result.TopCommenters.ShouldNotBeNull();

        result.TopProposers.Count.ShouldBeGreaterThan(0);
        result.TopVoters.Count.ShouldBeGreaterThan(0);
        result.TopCommenters.Count.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task GetFrustrationBarometerAsync_ShouldReturnFrustrationData()
    {
        // Act
        var result = await _analyticsService.GetFrustrationBarometerAsync();

        // Assert
        result.ShouldNotBeNull();
        result.FrustrationLevel.ShouldBe(33.33, 0.01); // 2 against votes out of 6 total
        result.TotalVotes.ShouldBe(6);
        result.TotalVotesAgainst.ShouldBe(2);
        result.CurrentMood.ShouldBe("Légèrement frustré");
        result.CategoryBreakdown.ShouldNotBeNull();
    }

    [Test]
    public async Task GetRecentActivityAsync_ShouldReturnRecentActivity()
    {
        // Act
        var result = await _analyticsService.GetRecentActivityAsync(20);

        // Assert
        result.ShouldNotBeNull();
        result.Activities.ShouldNotBeNull();
        result.Activities.Count.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task GetVotingTrendsAsync_ShouldReturnVotingTrends()
    {
        // Act
        var result = await _analyticsService.GetVotingTrendsAsync(30);

        // Assert
        result.ShouldNotBeNull();
        result.DailyVotes.ShouldNotBeNull();
        result.DailyVotes.Count.ShouldBeGreaterThan(0);
    }

    private void SeedTestData()
    {
        // Add test users
        var users = new List<ApplicationUser>
        {
            new() { Id = "user1", DisplayName = "User 1", ContributionLevel = ContributionLevel.PetitNicolas, ReputationScore = 100, LastLoginAt = DateTime.UtcNow.AddDays(-5) },
            new() { Id = "user2", DisplayName = "User 2", ContributionLevel = ContributionLevel.PetitNicolas, ReputationScore = 150, LastLoginAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = "user3", DisplayName = "User 3", ContributionLevel = ContributionLevel.GrosMoyenNicolas, ReputationScore = 250, LastLoginAt = DateTime.UtcNow.AddDays(-20) },
            new() { Id = "user4", DisplayName = "User 4", ContributionLevel = ContributionLevel.GrosNicolas, ReputationScore = 500, LastLoginAt = DateTime.UtcNow.AddDays(-50) },
            new() { Id = "user5", DisplayName = "User 5", ContributionLevel = ContributionLevel.NicolasSupreme, ReputationScore = 1000, LastLoginAt = DateTime.UtcNow.AddDays(-100) }
        };

        // Add test categories
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Fiscalité", IsActive = true, Color = "#FF6B6B", IconClass = "fas fa-coins" },
            new() { Id = 2, Name = "Social", IsActive = true, Color = "#4ECDC4", IconClass = "fas fa-users" },
            new() { Id = 3, Name = "Économie", IsActive = true, Color = "#45B7D1", IconClass = "fas fa-chart-line" }
        };

        // Add test proposals
        var proposals = new List<Proposal>
        {
            new() { Id = 1, Title = "Test Proposal 1", Description = "Description 1", CreatedById = "user1", CategoryId = 1, Status = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 2, Title = "Test Proposal 2", Description = "Description 2", CreatedById = "user2", CategoryId = 2, Status = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus.Active, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 3, Title = "Test Proposal 3", Description = "Description 3", CreatedById = "user3", CategoryId = 3, Status = NicolasQuiPaieAPI.Infrastructure.Models.ProposalStatus.Closed, CreatedAt = DateTime.UtcNow.AddDays(-3) }
        };

        // Add test votes
        var votes = new List<Vote>
        {
            new() { Id = 1, UserId = "user1", ProposalId = 1, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, Weight = 1, VotedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 2, UserId = "user2", ProposalId = 1, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.Against, Weight = 1, VotedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = 3, UserId = "user3", ProposalId = 2, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, Weight = 2, VotedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 4, UserId = "user4", ProposalId = 2, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, Weight = 3, VotedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 5, UserId = "user5", ProposalId = 3, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.Against, Weight = 5, VotedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = 6, UserId = "user1", ProposalId = 3, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, Weight = 1, VotedAt = DateTime.UtcNow.AddDays(-3) }
        };

        // Add test comments
        var comments = new List<Comment>
        {
            new() { Id = 1, UserId = "user1", ProposalId = 1, Content = "Comment 1", CreatedAt = DateTime.UtcNow.AddDays(-1), IsDeleted = false },
            new() { Id = 2, UserId = "user2", ProposalId = 1, Content = "Comment 2", CreatedAt = DateTime.UtcNow.AddDays(-1), IsDeleted = false },
            new() { Id = 3, UserId = "user3", ProposalId = 2, Content = "Comment 3", CreatedAt = DateTime.UtcNow.AddDays(-2), IsDeleted = false },
            new() { Id = 4, UserId = "user4", ProposalId = 2, Content = "Comment 4", CreatedAt = DateTime.UtcNow.AddDays(-2), IsDeleted = false }
        };

        _context.Users.AddRange(users);
        _context.Categories.AddRange(categories);
        _context.Proposals.AddRange(proposals);
        _context.Votes.AddRange(votes);
        _context.Comments.AddRange(comments);
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}