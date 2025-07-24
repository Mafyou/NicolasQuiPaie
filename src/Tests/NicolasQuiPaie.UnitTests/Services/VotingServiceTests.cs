// C# 13.0 - Enhanced Voting Service Tests with latest language features - Democratic Edition

using AutoMapper;
using Microsoft.Extensions.Logging;
using NicolasQuiPaie.UnitTests.Helpers;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieAPI.Application.Services;
using NicolasQuiPaieAPI.Infrastructure.Models;
using NicolasQuiPaieData.DTOs;
using InfrastructureContributionLevel = NicolasQuiPaieAPI.Infrastructure.Models.ContributionLevel;

namespace Tests.NicolasQuiPaie.UnitTests.Services;

/// <summary>
/// C# 13.0 - Democratic voting service tests ensuring equal voting rights for all Nicolas
/// </summary>
[TestFixture]
public class VotingServiceTests
{
    // C# 13.0 - Field keyword for backing fields (simulated with private fields until full support)
    private Mock<IUnitOfWork> _mockUnitOfWork = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<ILogger<VotingService>> _mockLogger = null!;
    private Mock<IVoteRepository> _mockVoteRepository = null!;
    private Mock<IProposalRepository> _mockProposalRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private VotingService _votingService = null!;

    [SetUp]
    public void Setup()
    {
        // C# 13.0 - Tuple deconstruction with modern setup
        (_mockUnitOfWork, _mockMapper, _mockLogger) = TestDataHelper.CreateServiceMocks<VotingService>();

        _mockVoteRepository = TestDataHelper.CreateMockWithDefaults<IVoteRepository>();
        _mockProposalRepository = TestDataHelper.CreateMockWithDefaults<IProposalRepository>();
        _mockUserRepository = TestDataHelper.CreateMockWithDefaults<IUserRepository>();

        // Setup repository properties
        _mockUnitOfWork.Setup(x => x.Votes).Returns(_mockVoteRepository.Object);
        _mockUnitOfWork.Setup(x => x.Proposals).Returns(_mockProposalRepository.Object);
        _mockUnitOfWork.Setup(x => x.Users).Returns(_mockUserRepository.Object);

        // Setup transaction methods
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.RollbackTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        _votingService = new VotingService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockUserRepository.Object);
    }

    // C# 13.0 - Collection expressions in test data - All votes are equal!
    [Test]
    [TestCaseSource(nameof(ContributionLevelTestCases))]
    public async Task CastVoteAsync_ShouldApplyEqualWeight_ForAllContributionLevels(
        InfrastructureContributionLevel contributionLevel, int expectedWeight)
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[0];
        var proposalId = 1;

        var user = TestDataHelper.CreateTestUser(userId, contributionLevel: contributionLevel);
        var proposal = TestDataHelper.CreateTestProposals(("Test Proposal", userId, 1))[0];

        var createVoteDto = new CreateVoteDto
        {
            ProposalId = proposalId,
            VoteType = NicolasQuiPaieData.DTOs.VoteType.For,
            Comment = "Test vote comment"
        };

        var expectedVote = new Vote
        {
            Id = 1,
            VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 1, // Always 1 for democratic equality
            VotedAt = DateTime.UtcNow,
            Comment = createVoteDto.Comment
        };

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                          .ReturnsAsync(user);

        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
                              .ReturnsAsync(proposal);

        _mockVoteRepository.Setup(x => x.GetUserVoteForProposalAsync(userId, proposalId))
                          .ReturnsAsync((Vote?)null);

        _mockVoteRepository.Setup(x => x.AddAsync(It.IsAny<Vote>()))
                          .ReturnsAsync(expectedVote);

        _mockProposalRepository.Setup(x => x.UpdateVoteCountsAsync(proposalId))
                              .Returns(Task.CompletedTask);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                          .ReturnsAsync(user);

        // Act
        var result = await _votingService.CastVoteAsync(createVoteDto, userId);

        // Assert
        result.ShouldNotBeNull();
        result.Weight.ShouldBe(1); // Democratic equality: all votes weigh the same
        result.UserId.ShouldBe(userId);
        result.VoteType.ShouldBe(NicolasQuiPaieData.DTOs.VoteType.For);

        _mockVoteRepository.Verify(x => x.AddAsync(
            It.Is<Vote>(v => v.Weight == 1 && v.UserId == userId)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    // C# 13.0 - Collection expressions for equal democratic voting test cases
    public static readonly object[][] ContributionLevelTestCases =
    [
        [InfrastructureContributionLevel.PetitNicolas, 1],      // Equal weight
        [InfrastructureContributionLevel.GrosMoyenNicolas, 1],  // Equal weight
        [InfrastructureContributionLevel.GrosNicolas, 1],       // Equal weight
        [InfrastructureContributionLevel.NicolasSupreme, 1]     // Equal weight
    ];

    [Test]
    public async Task CastVoteAsync_ShouldUpdateExistingVote_WhenUserHasAlreadyVoted()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[1];
        var proposalId = 2;

        var user = TestDataHelper.CreateTestUser(userId, contributionLevel: InfrastructureContributionLevel.GrosMoyenNicolas);
        var proposal = TestDataHelper.CreateTestProposals(("Existing Vote Proposal", userId, 1))[0];

        var existingVote = new Vote
        {
            Id = 1,
            VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.Against,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 1, // Democratic equality
            VotedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateVoteDto = new CreateVoteDto
        {
            ProposalId = proposalId,
            VoteType = NicolasQuiPaieData.DTOs.VoteType.For, // Changing from Against to For
            Comment = "Updated vote comment"
        };

        // Updated vote still has weight = 1
        var updatedVote = new Vote
        {
            Id = existingVote.Id,
            VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For,
            UserId = existingVote.UserId,
            ProposalId = existingVote.ProposalId,
            Weight = 1, // Democratic equality
            VotedAt = DateTime.UtcNow,
            Comment = updateVoteDto.Comment
        };

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                      .ReturnsAsync(user);

        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
                          .ReturnsAsync(proposal);

        _mockVoteRepository.Setup(x => x.GetUserVoteForProposalAsync(userId, proposalId))
                      .ReturnsAsync(existingVote);

        _mockVoteRepository.Setup(x => x.UpdateAsync(It.IsAny<Vote>()))
                      .ReturnsAsync(updatedVote);

        _mockProposalRepository.Setup(x => x.UpdateVoteCountsAsync(proposalId))
                              .Returns(Task.CompletedTask);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                          .ReturnsAsync(user);

        // Act
        var result = await _votingService.CastVoteAsync(updateVoteDto, userId);

        // Assert
        result.ShouldNotBeNull();
        result.VoteType.ShouldBe(NicolasQuiPaieData.DTOs.VoteType.For);
        result.Weight.ShouldBe(1); // Democratic equality

        _mockVoteRepository.Verify(x => x.UpdateAsync(
            It.Is<Vote>(v => v.VoteType == NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For && v.Comment == updateVoteDto.Comment && v.Weight == 1)), Times.Once);
        _mockVoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    // C# 13.0 - Params collections for flexible error testing with modern null patterns
    [Test]
    [TestCase("", "Proposal ID must be provided")]
    [TestCase("invalid-user", "User not found")]
    public async Task CastVoteAsync_ShouldThrowException_WithInvalidInput(
        string invalidUserId, string expectedErrorPattern)
    {
        // Arrange
        var createVoteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = NicolasQuiPaieData.DTOs.VoteType.For
        };

        if (string.IsNullOrEmpty(invalidUserId))
        {
            // Test argument validation for null or empty
            var exception = await Should.ThrowAsync<ArgumentException>(
                () => _votingService.CastVoteAsync(createVoteDto, invalidUserId));

            exception.Message.ShouldContain("userId");
        }
        else
        {
            // Test user not found scenario
            _mockUserRepository.Setup(x => x.GetByIdAsync(invalidUserId))
                          .ReturnsAsync((ApplicationUser?)null);

            var exception = await Should.ThrowAsync<InvalidOperationException>(
                () => _votingService.CastVoteAsync(createVoteDto, invalidUserId));

            exception.Message.ShouldContain("User not found");
            _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
        }
    }

    [Test]
    public async Task CastVoteAsync_ShouldThrowException_WithWhitespaceUserId()
    {
        // Arrange
        var createVoteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = NicolasQuiPaieData.DTOs.VoteType.For
        };

        // Test whitespace validation - should still work since whitespace is a valid string that returns null user
        _mockUserRepository.Setup(x => x.GetByIdAsync("   "))
                      .ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _votingService.CastVoteAsync(createVoteDto, "   "));

        // The actual exception message includes the exact userId, so check for the pattern
        exception.Message.ShouldContain("not found");
        _mockUnitOfWork.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    [Test]
    public async Task GetUserVoteForProposalAsync_ShouldReturnVoteDto_WhenVoteExists()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[0];
        var proposalId = 1;

        var existingVote = new Vote
        {
            Id = 1,
            VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 1, // Democratic equality
            VotedAt = DateTime.UtcNow.AddHours(-2),
            Comment = "Great proposal!"
        };

        _mockVoteRepository.Setup(x => x.GetUserVoteForProposalAsync(userId, proposalId))
                          .ReturnsAsync(existingVote);

        // Act
        var result = await _votingService.GetUserVoteForProposalAsync(userId, proposalId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingVote.Id);
        result.VoteType.ShouldBe(NicolasQuiPaieData.DTOs.VoteType.For);
        result.Weight.ShouldBe(1); // Democratic equality
        result.Comment.ShouldBe("Great proposal!");
    }

    [Test]
    public async Task GetUserVoteAsync_ShouldReturnNull_WhenVoteDoesNotExist()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[2];
        var proposalId = 999;

        // C# 13.0 - Modern null return pattern
        _mockVoteRepository.Setup(x => x.GetUserVoteForProposalAsync(userId, proposalId))
                          .ReturnsAsync((Vote?)null);

        // Act
        var result = await _votingService.GetUserVoteForProposalAsync(userId, proposalId);

        // Assert - C# 13.0 modern null checking
        result.ShouldBeNull();
    }

    // C# 13.0 - Modern async test with collection expressions
    [Test]
    public async Task GetVotesForProposalAsync_ShouldReturnAllVotes_ForSpecificProposal()
    {
        // Arrange
        var proposalId = 1;
        var votes = new List<Vote>
        {
            new() { Id = 1, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, UserId = "user1", ProposalId = proposalId, Weight = 1, VotedAt = DateTime.UtcNow },
            new() { Id = 2, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.Against, UserId = "user2", ProposalId = proposalId, Weight = 1, VotedAt = DateTime.UtcNow },
            new() { Id = 3, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, UserId = "user3", ProposalId = proposalId, Weight = 1, VotedAt = DateTime.UtcNow }
        };

        _mockVoteRepository.Setup(x => x.GetVotesForProposalAsync(proposalId))
                          .ReturnsAsync(votes);

        // Act
        var result = await _votingService.GetVotesForProposalAsync(proposalId);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        // Count the actual "For" votes (2 out of 3)
        result.Count(v => v.VoteType == NicolasQuiPaieData.DTOs.VoteType.For).ShouldBe(2);
        result.Sum(v => v.Weight).ShouldBe(3); // All weights are 1: 1 + 1 + 1 = 3
        
        // Verify democratic equality: all votes should have weight = 1
        result.All(v => v.Weight == 1).ShouldBeTrue();
    }

    [Test]
    public async Task GetVotesForProposalAsync_ShouldShowDemocraticEquality_InVoteDistribution()
    {
        // Arrange - Test scenario with different contribution levels but equal voting power
        var proposalId = 1;
        var votes = new List<Vote>
        {
            // Petit Nicolas vote
            new() { Id = 1, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, UserId = "petit-nicolas", ProposalId = proposalId, Weight = 1, VotedAt = DateTime.UtcNow },
            // Nicolas Supreme vote - same weight!
            new() { Id = 2, VoteType = NicolasQuiPaieAPI.Infrastructure.Models.VoteType.For, UserId = "nicolas-supreme", ProposalId = proposalId, Weight = 1, VotedAt = DateTime.UtcNow }
        };

        _mockVoteRepository.Setup(x => x.GetVotesForProposalAsync(proposalId))
                          .ReturnsAsync(votes);

        // Act
        var result = await _votingService.GetVotesForProposalAsync(proposalId);

        // Assert - Democratic principle verified
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.All(v => v.Weight == 1).ShouldBeTrue("All Nicolas should have equal voting power");
        result.Sum(v => v.Weight).ShouldBe(2, "Total vote count should equal number of voters (democratic equality)");
    }

    [TearDown]
    public void TearDown()
    {
        // C# 13.0 - Enhanced cleanup with null conditional operators
        _mockUnitOfWork?.Reset();
        _mockMapper?.Reset();
        _mockLogger?.Reset();
        _mockVoteRepository?.Reset();
        _mockProposalRepository?.Reset();
        _mockUserRepository?.Reset();
    }
}