namespace NicolasQuiPaie.UnitTests.Services;

public class ProposalServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<ILogger<ProposalService>> _mockLogger;
    private Mock<IProposalRepository> _mockProposalRepository;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private ProposalService _proposalService;

    public ProposalServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ProposalService>>();
        _mockProposalRepository = new Mock<IProposalRepository>();

        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockCategoryRepository.Setup(x => x.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

        _mockUnitOfWork.Setup(x => x.Proposals).Returns(_mockProposalRepository.Object);
        _mockUnitOfWork.Setup(x => x.Categories).Returns(_mockCategoryRepository.Object);

        _proposalService = new ProposalService(
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetActiveProposalsAsync_ShouldReturnProposals_WhenProposalsExist()
    {
        // Arrange
        var proposals = new List<Proposal>
        {
            new Proposal { Id = 1, Title = "Test Proposal 1", Status = InfrastructureProposalStatus.Active },
            new Proposal { Id = 2, Title = "Test Proposal 2", Status = InfrastructureProposalStatus.Active }
        };

        var proposalDtos = new List<ProposalDto>
        {
            new ProposalDto { Id = 1, Title = "Test Proposal 1", Status = DtoProposalStatus.Active },
            new ProposalDto { Id = 2, Title = "Test Proposal 2", Status = DtoProposalStatus.Active }
        };

        _mockProposalRepository
            .Setup(x => x.GetActiveProposalsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>()))
            .ReturnsAsync(proposals);

        // Act
        var result = await _proposalService.GetActiveProposalsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
        result.First().Title.ShouldBe("Test Proposal 1");

        _mockProposalRepository.Verify(x => x.GetActiveProposalsAsync(0, 20, null, null), Times.Once);
    }

    [Fact]
    public async Task GetProposalByIdAsync_ShouldReturnProposal_WhenProposalExists()
    {
        // Arrange
        var proposalId = 1;
        var proposal = new Proposal { Id = proposalId, Title = "Test Proposal", Status = InfrastructureProposalStatus.Active };
        var proposalDto = new ProposalDto { Id = proposalId, Title = "Test Proposal", Status = DtoProposalStatus.Active };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        // Act
        var result = await _proposalService.GetProposalByIdAsync(proposalId);

        // Assert
        var dto = result switch
        {
            ProposalDto found => found,
            _ => throw new ShouldAssertException($"Expected the proposal, got {result}")
        };
        dto.Id.ShouldBe(proposalId);
        dto.Title.ShouldBe("Test Proposal");

        _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
    }

    [Fact]
    public async Task GetProposalByIdAsync_ShouldReturnNotFound_WhenProposalDoesNotExist()
    {
        // Arrange
        var proposalId = 999;

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync((Proposal?)null);

        // Act
        var result = await _proposalService.GetProposalByIdAsync(proposalId);

        // Assert
        (result is ProposalNotFound { ProposalId: 999 }).ShouldBeTrue();

        _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
    }

    [Fact]
    public async Task CreateProposalAsync_ShouldCreateProposal_WhenValidDataProvided()
    {
        // Arrange
        var userId = "user123";
        var createDto = new CreateProposalDto
        {
            Title = "New Proposal",
            Description = "This is a test proposal description that is long enough to pass validation.",
            CategoryId = 1
        };

        var proposal = new Proposal
        {
            Title = createDto.Title,
            Description = createDto.Description,
            CategoryId = createDto.CategoryId,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active
        };

        var createdProposal = new Proposal
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description,
            CategoryId = createDto.CategoryId,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var proposalDto = new ProposalDto
        {
            Id = 1,
            Title = createDto.Title,
            Description = createDto.Description,
            CategoryId = createDto.CategoryId,
            CreatedById = userId,
            Status = DtoProposalStatus.Active
        };

        _mockProposalRepository
            .Setup(x => x.AddAsync(It.IsAny<Proposal>()))
            .ReturnsAsync(createdProposal);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var creation = await _proposalService.CreateProposalAsync(createDto, userId);

        // Assert
        var result = creation switch
        {
            ProposalDto created => created,
            _ => throw new ShouldAssertException($"Expected the created proposal, got {creation}")
        };
        result.Title.ShouldBe(createDto.Title);
        result.CreatedById.ShouldBe(userId);
        result.Status.ShouldBe(DtoProposalStatus.Active);

        _mockProposalRepository.Verify(x => x.AddAsync(It.Is<Proposal>(p =>
            p.Title == createDto.Title &&
            p.CreatedById == userId &&
            p.Status == InfrastructureProposalStatus.Active)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateProposalAsync_ShouldRefuseAndSaveNothing_WhenCategoryDoesNotExist()
    {
        // Arrange
        _mockCategoryRepository.Setup(x => x.ExistsAsync(42)).ReturnsAsync(false);
        var createDto = new CreateProposalDto
        {
            Title = "Proposal in a category that does not exist",
            Description = "This proposal points to a category id that no row of the categories table has.",
            CategoryId = 42
        };

        // Act
        var result = await _proposalService.CreateProposalAsync(createDto, "user123");

        // Assert
        (result is UnknownCategory { CategoryId: 42 }).ShouldBeTrue();
        _mockProposalRepository.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateProposalAsync_ShouldUpdateProposal_WhenUserIsOwner()
    {
        // Arrange
        var proposalId = 1;
        var userId = "user123";
        var updateDto = new UpdateProposalDto
        {
            Title = "Updated Proposal",
            Description = "This is an updated test proposal description that is long enough.",
            CategoryId = 2
        };

        var existingProposal = new Proposal
        {
            Id = proposalId,
            Title = "Original Proposal",
            Description = "Original description that is long enough to pass validation.",
            CategoryId = 1,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var updatedProposal = new Proposal
        {
            Id = proposalId,
            Title = updateDto.Title,
            Description = updateDto.Description,
            CategoryId = updateDto.CategoryId,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        var proposalDto = new ProposalDto
        {
            Id = proposalId,
            Title = updateDto.Title,
            Description = updateDto.Description,
            CategoryId = updateDto.CategoryId
        };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(existingProposal);

        _mockProposalRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Proposal>()))
            .ReturnsAsync(updatedProposal);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _proposalService.UpdateProposalAsync(proposalId, updateDto, userId);

        // Assert
        var updated = result switch
        {
            ProposalDto proposal => proposal,
            _ => throw new ShouldAssertException($"Expected the updated proposal, got {result}")
        };
        updated.Title.ShouldBe(updateDto.Title);

        _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
        _mockProposalRepository.Verify(x => x.UpdateAsync(It.IsAny<Proposal>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProposalAsync_ShouldRefuse_WhenUserIsNotOwner()
    {
        // Arrange
        var proposalId = 1;
        var userId = "user123";
        var differentUserId = "user456";
        var updateDto = new UpdateProposalDto
        {
            Title = "Updated Proposal",
            Description = "This is an updated test proposal description that is long enough.",
            CategoryId = 2
        };

        var existingProposal = new Proposal
        {
            Id = proposalId,
            CreatedById = differentUserId, // Different user
            Status = InfrastructureProposalStatus.Active
        };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(existingProposal);

        // Act
        var result = await _proposalService.UpdateProposalAsync(proposalId, updateDto, userId);

        // Assert
        (result is NotProposalAuthor { ProposalId: 1, UserId: "user123" }).ShouldBeTrue();

        _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
        _mockProposalRepository.Verify(x => x.UpdateAsync(It.IsAny<Proposal>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteProposalAsync_ShouldDeleteProposal_WhenUserIsOwner()
    {
        // Arrange
        var proposalId = 1;
        var userId = "user123";

        var existingProposal = new Proposal
        {
            Id = proposalId,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active
        };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(existingProposal);

        _mockProposalRepository
            .Setup(x => x.DeleteAsync(proposalId))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _proposalService.DeleteProposalAsync(proposalId, userId);

        // Assert
        (result is ProposalDeleted).ShouldBeTrue();
        _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
        _mockProposalRepository.Verify(x => x.DeleteAsync(proposalId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CanUserEditProposalAsync_ShouldReturnTrue_WhenUserIsOwner()
    {
        // Arrange
        var proposalId = 1;
        var userId = "user123";

        var proposal = new Proposal
        {
            Id = proposalId,
            CreatedById = userId,
            Status = InfrastructureProposalStatus.Active
        };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        // Act
        var result = await _proposalService.CanUserEditProposalAsync(proposalId, userId);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task CanUserEditProposalAsync_ShouldReturnFalse_WhenUserIsNotOwner()
    {
        // Arrange
        var proposalId = 1;
        var userId = "user123";
        var differentUserId = "user456";

        var proposal = new Proposal
        {
            Id = proposalId,
            CreatedById = differentUserId,
            Status = InfrastructureProposalStatus.Active
        };

        _mockProposalRepository
            .Setup(x => x.GetByIdAsync(proposalId))
            .ReturnsAsync(proposal);

        // Act
        var result = await _proposalService.CanUserEditProposalAsync(proposalId, userId);

        // Assert
        result.ShouldBeFalse();
    }
}