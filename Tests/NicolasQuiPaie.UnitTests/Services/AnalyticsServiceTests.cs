using AutoMapper;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieAPI.Application.Services;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaie.UnitTests.Services;

[TestFixture]
public class AnalyticsServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<AnalyticsService>> _mockLogger;
    private Mock<IProposalRepository> _mockProposalRepository;
    private Mock<IVoteRepository> _mockVoteRepository;
    private Mock<IUserRepository> _mockUserRepository;
    private Mock<ICommentRepository> _mockCommentRepository;
    private AnalyticsService _analyticsService;

    [SetUp]
    public void SetUp()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<AnalyticsService>>();
        _mockProposalRepository = new Mock<IProposalRepository>();
        _mockVoteRepository = new Mock<IVoteRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockCommentRepository = new Mock<ICommentRepository>();

        _mockUnitOfWork.Setup(x => x.Proposals).Returns(_mockProposalRepository.Object);
        _mockUnitOfWork.Setup(x => x.Votes).Returns(_mockVoteRepository.Object);
        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(x => x.Comments).Returns(_mockCommentRepository.Object);

        _analyticsService = new AnalyticsService(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task GetDashboardStatsAsync_ShouldReturnCompleteStats()
    {
        // Arrange
        var mockUsers = CreateMockUsers();
        var mockVotes = CreateMockVotes();
        var mockProposals = CreateMockProposals();
        var mockComments = CreateMockComments();

        _mockUserRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(mockUsers);

        _mockVoteRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(mockVotes);

        _mockProposalRepository.Setup(x => x.GetActiveProposalsAsync(0, int.MaxValue, null, null))
            .ReturnsAsync(mockProposals);

        _mockCommentRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(mockComments);

        // Act
        var result = await _analyticsService.GetDashboardStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.TotalUsers.ShouldBe(4); // 4 mock users
        result.TotalVotes.ShouldBe(6); // 6 mock votes
        result.ActiveProposals.ShouldBe(2); // 2 active proposals
        result.TotalComments.ShouldBe(3); // 3 mock comments
        result.RasLebolMeter.ShouldBeGreaterThan(0);
        result.RasLebolMeter.ShouldBeLessThanOrEqualTo(100);
    }

    [Test]
    public async Task GetVotingTrendsAsync_ShouldReturnTrendsForPeriod()
    {
        // Arrange
        var mockVotes = CreateMockVotesWithDates();
        _mockVoteRepository.Setup(x => x.GetVotesInPeriodAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(mockVotes);

        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var result = await _analyticsService.GetVotingTrendsAsync(startDate, endDate);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBeGreaterThan(0);
        result.All(trend => trend.Date >= startDate && trend.Date <= endDate).ShouldBeTrue();

        _mockVoteRepository.Verify(x => x.GetVotesInPeriodAsync(startDate, endDate), Times.Once);
    }

    [Test]
    public async Task GetFiscalDistributionAsync_ShouldReturnCorrectDistribution()
    {
        // Arrange
        var mockUsers = CreateMockUsersWithDifferentLevels();
        _mockUserRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _analyticsService.GetFiscalDistributionAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(4); // One for each fiscal level

        var petitNicolas = result.FirstOrDefault(x => x.FiscalLevel == FiscalLevel.PetitNicolas);
        petitNicolas.ShouldNotBeNull();
        petitNicolas.Count.ShouldBe(2); // 2 users at this level

        var nicolasSupreme = result.FirstOrDefault(x => x.FiscalLevel == FiscalLevel.NicolasSupreme);
        nicolasSupreme.ShouldNotBeNull();
        nicolasSupreme.Count.ShouldBe(1); // 1 user at this level
    }

    [Test]
    public async Task GetTopContributorsAsync_ShouldReturnUsersOrderedByReputation()
    {
        // Arrange
        var mockUsers = CreateMockUsersWithDifferentReputation();
        _mockUserRepository.Setup(x => x.GetTopContributorsAsync(10))
            .ReturnsAsync(mockUsers.OrderByDescending(u => u.ReputationScore).Take(10));

        // Act
        var result = await _analyticsService.GetTopContributorsAsync(10);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBeLessThanOrEqualTo(10);

        // Should be ordered by reputation descending
        var resultList = result.ToList();
        for (int i = 0; i < resultList.Count - 1; i++)
        {
            resultList[i].ReputationScore.ShouldBeGreaterThanOrEqualTo(resultList[i + 1].ReputationScore);
        }

        _mockUserRepository.Verify(x => x.GetTopContributorsAsync(10), Times.Once);
    }

    [Test]
    public async Task CalculateRasLebolMeterAsync_ShouldReturnPercentageBetween0And100()
    {
        // Arrange
        var mockVotes = CreateMockVotesForRasLebol();
        _mockVoteRepository.Setup(x => x.GetRecentVotesAsync(It.IsAny<int>()))
            .ReturnsAsync(mockVotes);

        // Act
        var result = await _analyticsService.CalculateRasLebolMeterAsync();

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(0);
        result.ShouldBeLessThanOrEqualTo(100);

        _mockVoteRepository.Verify(x => x.GetRecentVotesAsync(It.IsAny<int>()), Times.Once);
    }

    [Test]
    public async Task GetProposalAnalyticsAsync_ShouldReturnDetailedProposalStats()
    {
        // Arrange
        var proposalId = 1;
        var mockProposal = CreateMockProposal(proposalId);
        var mockVotes = CreateMockVotesForProposal(proposalId);
        var mockComments = CreateMockCommentsForProposal(proposalId);

        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(mockProposal);

        _mockVoteRepository.Setup(x => x.GetVotesForProposalAsync(proposalId))
            .ReturnsAsync(mockVotes);

        _mockCommentRepository.Setup(x => x.GetCommentsForProposalAsync(proposalId))
            .ReturnsAsync(mockComments);

        // Act
        var result = await _analyticsService.GetProposalAnalyticsAsync(proposalId);

        // Assert
        result.ShouldNotBeNull();
        result.ProposalId.ShouldBe(proposalId);
        result.TotalVotes.ShouldBe(mockVotes.Count());
        result.TotalComments.ShouldBe(mockComments.Count());
        result.VotesFor.ShouldBe(mockVotes.Count(v => v.VoteType == Infrastructure.Models.VoteType.For));
        result.VotesAgainst.ShouldBe(mockVotes.Count(v => v.VoteType == Infrastructure.Models.VoteType.Against));
    }

    [Test]
    public async Task GetEngagementMetricsAsync_ShouldReturnUserEngagementData()
    {
        // Arrange
        var userId = "test-user-1";
        var mockUserVotes = CreateMockUserVotes(userId);
        var mockUserProposals = CreateMockUserProposals(userId);
        var mockUserComments = CreateMockUserComments(userId);

        _mockVoteRepository.Setup(x => x.GetUserVotesAsync(userId))
            .ReturnsAsync(mockUserVotes);

        _mockProposalRepository.Setup(x => x.GetUserProposalsAsync(userId))
            .ReturnsAsync(mockUserProposals);

        _mockCommentRepository.Setup(x => x.GetUserCommentsAsync(userId))
            .ReturnsAsync(mockUserComments);

        // Act
        var result = await _analyticsService.GetEngagementMetricsAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(userId);
        result.VotesCount.ShouldBe(mockUserVotes.Count());
        result.ProposalsCount.ShouldBe(mockUserProposals.Count());
        result.CommentsCount.ShouldBe(mockUserComments.Count());
        result.EngagementScore.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task GetDashboardStatsAsync_ShouldHandleEmptyData()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<ApplicationUser>());

        _mockVoteRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Vote>());

        _mockProposalRepository.Setup(x => x.GetActiveProposalsAsync(0, int.MaxValue, null, null))
            .ReturnsAsync(new List<Proposal>());

        _mockCommentRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Comment>());

        // Act
        var result = await _analyticsService.GetDashboardStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.TotalUsers.ShouldBe(0);
        result.TotalVotes.ShouldBe(0);
        result.ActiveProposals.ShouldBe(0);
        result.TotalComments.ShouldBe(0);
        result.RasLebolMeter.ShouldBe(0);
    }

    [Test]
    public async Task GetProposalAnalyticsAsync_ShouldReturnNull_WhenProposalNotFound()
    {
        // Arrange
        var proposalId = 999;
        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync((Proposal?)null);

        // Act
        var result = await _analyticsService.GetProposalAnalyticsAsync(proposalId);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CalculateRasLebolMeterAsync_ShouldReturn0_WhenNoVotes()
    {
        // Arrange
        _mockVoteRepository.Setup(x => x.GetRecentVotesAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Vote>());

        // Act
        var result = await _analyticsService.CalculateRasLebolMeterAsync();

        // Assert
        result.ShouldBe(0);
    }

    #region Helper Methods

    private static List<ApplicationUser> CreateMockUsers()
    {
        return new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", FiscalLevel = FiscalLevel.PetitNicolas, ReputationScore = 100 },
            new ApplicationUser { Id = "user2", FiscalLevel = FiscalLevel.GrosMoyenNicolas, ReputationScore = 500 },
            new ApplicationUser { Id = "user3", FiscalLevel = FiscalLevel.GrosNicolas, ReputationScore = 1000 },
            new ApplicationUser { Id = "user4", FiscalLevel = FiscalLevel.NicolasSupreme, ReputationScore = 2000 }
        };
    }

    private static List<Vote> CreateMockVotes()
    {
        return new List<Vote>
        {
            new Vote { Id = 1, VoteType = Infrastructure.Models.VoteType.For, VotedAt = DateTime.UtcNow },
            new Vote { Id = 2, VoteType = Infrastructure.Models.VoteType.Against, VotedAt = DateTime.UtcNow },
            new Vote { Id = 3, VoteType = Infrastructure.Models.VoteType.For, VotedAt = DateTime.UtcNow },
            new Vote { Id = 4, VoteType = Infrastructure.Models.VoteType.For, VotedAt = DateTime.UtcNow },
            new Vote { Id = 5, VoteType = Infrastructure.Models.VoteType.Against, VotedAt = DateTime.UtcNow },
            new Vote { Id = 6, VoteType = Infrastructure.Models.VoteType.For, VotedAt = DateTime.UtcNow }
        };
    }

    private static List<Proposal> CreateMockProposals()
    {
        return new List<Proposal>
        {
            new Proposal { Id = 1, Status = ProposalStatus.Active, VotesFor = 10, VotesAgainst = 5 },
            new Proposal { Id = 2, Status = ProposalStatus.Active, VotesFor = 8, VotesAgainst = 12 }
        };
    }

    private static List<Comment> CreateMockComments()
    {
        return new List<Comment>
        {
            new Comment { Id = 1, ProposalId = 1, CreatedAt = DateTime.UtcNow },
            new Comment { Id = 2, ProposalId = 1, CreatedAt = DateTime.UtcNow },
            new Comment { Id = 3, ProposalId = 2, CreatedAt = DateTime.UtcNow }
        };
    }

    private static List<ApplicationUser> CreateMockUsersWithDifferentLevels()
    {
        return new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", FiscalLevel = FiscalLevel.PetitNicolas },
            new ApplicationUser { Id = "user2", FiscalLevel = FiscalLevel.PetitNicolas },
            new ApplicationUser { Id = "user3", FiscalLevel = FiscalLevel.GrosMoyenNicolas },
            new ApplicationUser { Id = "user4", FiscalLevel = FiscalLevel.GrosNicolas },
            new ApplicationUser { Id = "user5", FiscalLevel = FiscalLevel.NicolasSupreme }
        };
    }

    private static List<ApplicationUser> CreateMockUsersWithDifferentReputation()
    {
        return new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", ReputationScore = 2000 },
            new ApplicationUser { Id = "user2", ReputationScore = 1500 },
            new ApplicationUser { Id = "user3", ReputationScore = 1000 },
            new ApplicationUser { Id = "user4", ReputationScore = 500 }
        };
    }

    private static List<Vote> CreateMockVotesWithDates()
    {
        var now = DateTime.UtcNow;
        return new List<Vote>
        {
            new Vote { Id = 1, VotedAt = now.AddDays(-1) },
            new Vote { Id = 2, VotedAt = now.AddDays(-2) },
            new Vote { Id = 3, VotedAt = now.AddDays(-3) }
        };
    }

    private static List<Vote> CreateMockVotesForRasLebol()
    {
        return new List<Vote>
        {
            new Vote { VoteType = Infrastructure.Models.VoteType.Against },
            new Vote { VoteType = Infrastructure.Models.VoteType.Against },
            new Vote { VoteType = Infrastructure.Models.VoteType.Against },
            new Vote { VoteType = Infrastructure.Models.VoteType.For }
        };
    }

    private static Proposal CreateMockProposal(int id)
    {
        return new Proposal
        {
            Id = id,
            Title = $"Test Proposal {id}",
            Status = ProposalStatus.Active,
            VotesFor = 10,
            VotesAgainst = 5,
            ViewsCount = 100
        };
    }

    private static List<Vote> CreateMockVotesForProposal(int proposalId)
    {
        return new List<Vote>
        {
            new Vote { ProposalId = proposalId, VoteType = Infrastructure.Models.VoteType.For },
            new Vote { ProposalId = proposalId, VoteType = Infrastructure.Models.VoteType.For },
            new Vote { ProposalId = proposalId, VoteType = Infrastructure.Models.VoteType.Against }
        };
    }

    private static List<Comment> CreateMockCommentsForProposal(int proposalId)
    {
        return new List<Comment>
        {
            new Comment { ProposalId = proposalId, Content = "Comment 1" },
            new Comment { ProposalId = proposalId, Content = "Comment 2" }
        };
    }

    private static List<Vote> CreateMockUserVotes(string userId)
    {
        return new List<Vote>
        {
            new Vote { UserId = userId, VoteType = Infrastructure.Models.VoteType.For },
            new Vote { UserId = userId, VoteType = Infrastructure.Models.VoteType.Against }
        };
    }

    private static List<Proposal> CreateMockUserProposals(string userId)
    {
        return new List<Proposal>
        {
            new Proposal { CreatedById = userId, Title = "User Proposal 1" }
        };
    }

    private static List<Comment> CreateMockUserComments(string userId)
    {
        return new List<Comment>
        {
            new Comment { UserId = userId, Content = "User Comment 1" },
            new Comment { UserId = userId, Content = "User Comment 2" }
        };
    }

    #endregion
}