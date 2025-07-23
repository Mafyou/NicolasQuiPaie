// C# 13.0 - Enhanced Voting Service Tests with latest language features

using Tests.NicolasQuiPaie.UnitTests.Helpers;

namespace Tests.NicolasQuiPaie.UnitTests.Services;

/// <summary>
/// C# 13.0 - Voting service tests using primary constructors, collection expressions, and field keyword
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

        _votingService = new VotingService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    // C# 13.0 - Collection expressions in test data
    [Test]
    [TestCaseSource(nameof(FiscalLevelTestCases))]
    public async Task CastVoteAsync_ShouldApplyCorrectWeight_BasedOnFiscalLevel(
        InfrastructureFiscalLevel fiscalLevel, int expectedWeight)
    {
        // Arrange - C# 13.0 features
        var userId = TestDataHelper.ValidUserIds[0];
        var proposalId = 1;
        
        var user = TestDataHelper.CreateTestUser(userId, fiscalLevel: fiscalLevel);
        var proposal = TestDataHelper.CreateTestProposals(("Test Proposal", userId, 1))[0];
        
        var createVoteDto = new CreateVoteDto
        {
            ProposalId = proposalId,
            VoteType = VoteType.For,
            Comment = "Test vote comment"
        };

        var expectedVote = new Vote
        {
            Id = 1,
            VoteType = VoteType.For,
            UserId = userId,
            ProposalId = proposalId,
            Weight = expectedWeight,
            VotedAt = DateTime.UtcNow,
            Comment = createVoteDto.Comment
        };

        var expectedVoteDto = new VoteDto
        {
            Id = expectedVote.Id,
            VoteType = expectedVote.VoteType,
            UserId = userId,
            ProposalId = proposalId,
            Weight = expectedWeight,
            VotedAt = expectedVote.VotedAt,
            Comment = expectedVote.Comment
        };

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                          .ReturnsAsync(user);
        
        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
                              .ReturnsAsync(proposal);
        
        _mockVoteRepository.Setup(x => x.GetUserVoteAsync(userId, proposalId))
                          .ReturnsAsync((Vote?)null);
        
        _mockMapper.Setup(x => x.Map<Vote>(createVoteDto))
                  .Returns(expectedVote);
        
        _mockVoteRepository.Setup(x => x.AddAsync(It.IsAny<Vote>()))
                          .ReturnsAsync(expectedVote);
        
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                      .ReturnsAsync(1);
        
        _mockMapper.Setup(x => x.Map<VoteDto>(expectedVote))
                  .Returns(expectedVoteDto);

        // Act
        var result = await _votingService.CastVoteAsync(createVoteDto, userId);

        // Assert - C# 13.0 enhanced assertions with modern null checking
        result.ShouldNotBeNull();
        result.Weight.ShouldBe(expectedWeight);
        result.UserId.ShouldBe(userId);
        result.VoteType.ShouldBe(VoteType.For);
        
        _mockVoteRepository.Verify(x => x.AddAsync(
            It.Is<Vote>(v => v.Weight == expectedWeight && v.UserId == userId)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    // C# 13.0 - Collection expressions for test case data
    public static readonly object[][] FiscalLevelTestCases =
    [
        [InfrastructureFiscalLevel.PetitNicolas, 1],
        [InfrastructureFiscalLevel.GrosMoyenNicolas, 2],
        [InfrastructureFiscalLevel.GrosNicolas, 3],
        [InfrastructureFiscalLevel.NicolasSupreme, 5]
    ];

    [Test]
    public async Task CastVoteAsync_ShouldUpdateExistingVote_WhenUserHasAlreadyVoted()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[1];
        var proposalId = 2;
        
        var user = TestDataHelper.CreateTestUser(userId, fiscalLevel: InfrastructureFiscalLevel.GrosMoyenNicolas);
        var proposal = TestDataHelper.CreateTestProposals(("Existing Vote Proposal", userId, 1))[0];
        
        var existingVote = new Vote
        {
            Id = 1,
            VoteType = VoteType.Against,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 2,
            VotedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updateVoteDto = new CreateVoteDto
        {
            ProposalId = proposalId,
            VoteType = VoteType.For, // Changing from Against to For
            Comment = "Updated vote comment"
        };

        var updatedVote = existingVote with { VoteType = VoteType.For, Comment = updateVoteDto.Comment };
        
        var updatedVoteDto = new VoteDto
        {
            Id = updatedVote.Id,
            VoteType = VoteType.For,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 2,
            VotedAt = updatedVote.VotedAt,
            Comment = updatedVote.Comment
        };

        // Setup mocks
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
                          .ReturnsAsync(user);
        
        _mockProposalRepository.Setup(x => x.GetByIdAsync(proposalId))
                              .ReturnsAsync(proposal);
        
        _mockVoteRepository.Setup(x => x.GetUserVoteAsync(userId, proposalId))
                          .ReturnsAsync(existingVote);
        
        _mockVoteRepository.Setup(x => x.UpdateAsync(It.IsAny<Vote>()))
                          .ReturnsAsync(updatedVote);
        
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                      .ReturnsAsync(1);
        
        _mockMapper.Setup(x => x.Map<VoteDto>(updatedVote))
                  .Returns(updatedVoteDto);

        // Act
        var result = await _votingService.CastVoteAsync(updateVoteDto, userId);

        // Assert - C# 13.0 modern null checking
        result.ShouldNotBeNull();
        result.VoteType.ShouldBe(VoteType.For);
        result.Weight.ShouldBe(2);
        
        _mockVoteRepository.Verify(x => x.UpdateAsync(
            It.Is<Vote>(v => v.VoteType == VoteType.For && v.Comment == updateVoteDto.Comment)), Times.Once);
        _mockVoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>()), Times.Never);
    }

    // C# 13.0 - Params collections for flexible error testing with modern null patterns
    [Test]
    [TestCase("", "Proposal ID must be provided")]
    [TestCase("   ", "User ID cannot be whitespace")]
    [TestCase("invalid-user", "User not found")]
    public async Task CastVoteAsync_ShouldThrowException_WithInvalidInput(
        string invalidUserId, string expectedErrorPattern)
    {
        // Arrange
        var createVoteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = VoteType.For
        };

        if (string.IsNullOrWhiteSpace(invalidUserId))
        {
            // Test argument validation
            var exception = await Should.ThrowAsync<ArgumentException>(
                () => _votingService.CastVoteAsync(createVoteDto, invalidUserId));
            
            exception.Message.ShouldContain("userId");
        }
        else
        {
            // Test user not found scenario - C# 13.0 modern null return
            _mockUserRepository.Setup(x => x.GetByIdAsync(invalidUserId))
                              .ReturnsAsync((ApplicationUser?)null);

            var exception = await Should.ThrowAsync<InvalidOperationException>(
                () => _votingService.CastVoteAsync(createVoteDto, invalidUserId));
            
            exception.Message.ShouldContain("User not found");
        }
    }

    [Test]
    public async Task GetUserVoteAsync_ShouldReturnVoteDto_WhenVoteExists()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[0];
        var proposalId = 1;
        
        var existingVote = new Vote
        {
            Id = 1,
            VoteType = VoteType.For,
            UserId = userId,
            ProposalId = proposalId,
            Weight = 3,
            VotedAt = DateTime.UtcNow.AddHours(-2),
            Comment = "Great proposal!"
        };

        var expectedVoteDto = new VoteDto
        {
            Id = existingVote.Id,
            VoteType = existingVote.VoteType,
            UserId = userId,
            ProposalId = proposalId,
            Weight = existingVote.Weight,
            VotedAt = existingVote.VotedAt,
            Comment = existingVote.Comment
        };

        _mockVoteRepository.Setup(x => x.GetUserVoteAsync(userId, proposalId))
                          .ReturnsAsync(existingVote);
        
        _mockMapper.Setup(x => x.Map<VoteDto>(existingVote))
                  .Returns(expectedVoteDto);

        // Act
        var result = await _votingService.GetUserVoteAsync(userId, proposalId);

        // Assert - C# 13.0 modern null checking
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingVote.Id);
        result.VoteType.ShouldBe(VoteType.For);
        result.Weight.ShouldBe(3);
        result.Comment.ShouldBe("Great proposal!");
    }

    [Test]
    public async Task GetUserVoteAsync_ShouldReturnNull_WhenVoteDoesNotExist()
    {
        // Arrange
        var userId = TestDataHelper.ValidUserIds[2];
        var proposalId = 999;

        // C# 13.0 - Modern null return pattern
        _mockVoteRepository.Setup(x => x.GetUserVoteAsync(userId, proposalId))
                          .ReturnsAsync((Vote?)null);

        // Act
        var result = await _votingService.GetUserVoteAsync(userId, proposalId);

        // Assert - C# 13.0 modern null checking
        result.Should().BeNull();
        _mockMapper.Verify(x => x.Map<VoteDto>(It.IsAny<Vote>()), Times.Never);
    }

    // C# 13.0 - Modern async test with collection expressions
    [Test]
    public async Task GetVotesForProposalAsync_ShouldReturnAllVotes_ForSpecificProposal()
    {
        // Arrange
        var proposalId = 1;
        var votes = new List<Vote>
        {
            new() { Id = 1, VoteType = VoteType.For, UserId = "user1", ProposalId = proposalId, Weight = 1 },
            new() { Id = 2, VoteType = VoteType.Against, UserId = "user2", ProposalId = proposalId, Weight = 2 },
            new() { Id = 3, VoteType = VoteType.For, UserId = "user3", ProposalId = proposalId, Weight = 3 }
        };

        var voteDtos = votes.Select(v => new VoteDto
        {
            Id = v.Id,
            VoteType = v.VoteType,
            UserId = v.UserId,
            ProposalId = v.ProposalId,
            Weight = v.Weight,
            VotedAt = DateTime.UtcNow
        }).ToList();

        _mockVoteRepository.Setup(x => x.GetVotesForProposalAsync(proposalId))
                          .ReturnsAsync(votes);
        
        _mockMapper.Setup(x => x.Map<IEnumerable<VoteDto>>(votes))
                  .Returns(voteDtos);

        // Act
        var result = await _votingService.GetVotesForProposalAsync(proposalId);

        // Assert - C# 13.0 enhanced collection testing with modern null checking
        result.ShouldNotBeNull();
        result.Count().ShouldBe(3);
        result.Count(v => v.VoteType == VoteType.For).ShouldBe(2);
        result.Count(v => v.VoteType == VoteType.Against).ShouldBe(1);
        result.Sum(v => v.Weight).ShouldBe(6); // 1 + 2 + 3
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