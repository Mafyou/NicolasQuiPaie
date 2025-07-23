using AutoMapper;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieAPI.Application.Services;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaie.UnitTests.Services;

[TestFixture]
public class ProposalServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<ProposalService>> _mockLogger;
    private Mock<IProposalRepository> _mockProposalRepository;
    private Mock<ICategoryRepository> _mockCategoryRepository;
    private ProposalService _proposalService;
    private Proposal _testProposal;
    private CreateProposalDto _createProposalDto;
    private ApplicationUser _testUser;
    private Category _testCategory;

    [SetUp]
    public void SetUp()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<ProposalService>>();
        _mockProposalRepository = new Mock<IProposalRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();

        _mockUnitOfWork.Setup(x => x.Proposals).Returns(_mockProposalRepository.Object);
        _mockUnitOfWork.Setup(x => x.Categories).Returns(_mockCategoryRepository.Object);

        _proposalService = new ProposalService(
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);

        SetupTestData();
    }

    private void SetupTestData()
    {
        _testUser = new ApplicationUser
        {
            Id = "test-user-1",
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            DisplayName = "Test User",
            FiscalLevel = FiscalLevel.PetitNicolas,
            ReputationScore = 100,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        _testCategory = new Category
        {
            Id = 1,
            Name = "Fiscalité",
            Description = "Questions fiscales",
            Color = "#dc3545",
            IconClass = "fas fa-coins",
            IsActive = true
        };

        _testProposal = new Proposal
        {
            Id = 1,
            Title = "Test Proposal",
            Description = "Test proposal description",
            CategoryId = 1,
            Category = _testCategory,
            CreatedById = "test-user-1",
            CreatedBy = _testUser,
            Status = ProposalStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow,
            VotesFor = 10,
            VotesAgainst = 5,
            ViewsCount = 100,
            IsFeatured = false
        };

        _createProposalDto = new CreateProposalDto
        {
            Title = "New Test Proposal",
            Description = "New test proposal description",
            CategoryId = 1,
            Tags = "test,proposal"
        };
    }

    [Test]
    public async Task GetActiveProposalsAsync_ShouldReturnActiveProposals()
    {
        // Arrange
        var proposals = new List<Proposal> { _testProposal };
        var proposalDtos = new List<ProposalDto>
        {
            new ProposalDto
            {
                Id = 1,
                Title = "Test Proposal",
                Description = "Test proposal description",
                CategoryId = 1,
                CategoryName = "Fiscalité",
                CreatedByDisplayName = "Test User",
                Status = ProposalStatus.Active,
                VotesFor = 10,
                VotesAgainst = 5,
                ViewsCount = 100
            }
        };

        _mockProposalRepository.Setup(x => x.GetActiveProposalsAsync(0, 20, null, null))
            .ReturnsAsync(proposals);

        _mockMapper.Setup(x => x.Map<List<ProposalDto>>(proposals))
            .Returns(proposalDtos);

        // Act
        var result = await _proposalService.GetActiveProposalsAsync(0, 20);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.First().Title.ShouldBe("Test Proposal");
        result.First().Status.ShouldBe(ProposalStatus.Active);

        _mockProposalRepository.Verify(x => x.GetActiveProposalsAsync(0, 20, null, null), Times.Once);
    }

    [Test]
    public async Task GetTrendingProposalsAsync_ShouldReturnTrendingProposals()
    {
        // Arrange
        var proposals = new List<Proposal> { _testProposal };
        var proposalDtos = new List<ProposalDto>
        {
            new ProposalDto
            {
                Id = 1,
                Title = "Test Proposal",
                VotesFor = 10,
                VotesAgainst = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _mockProposalRepository.Setup(x => x.GetTrendingProposalsAsync(5))
            .ReturnsAsync(proposals);

        _mockMapper.Setup(x => x.Map<List<ProposalDto>>(proposals))
            .Returns(proposalDtos);

        // Act
        var result = await _proposalService.GetTrendingProposalsAsync(5);

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.First().TotalVotes.ShouldBe(15); // 10 + 5

        _mockProposalRepository.Verify(x => x.GetTrendingProposalsAsync(5), Times.Once);
    }

    [Test]
    public async Task GetProposalByIdAsync_ShouldReturnProposal_WhenExists()
    {
        // Arrange
        _mockProposalRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(_testProposal);

        var expectedDto = new ProposalDto
        {
            Id = 1,
            Title = "Test Proposal",
            Description = "Test proposal description"
        };

        _mockMapper.Setup(x => x.Map<ProposalDto>(_testProposal))
            .Returns(expectedDto);

        // Act
        var result = await _proposalService.GetProposalByIdAsync(1);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.Title.ShouldBe("Test Proposal");

        _mockProposalRepository.Verify(x => x.GetByIdAsync(1), Times.Once);
    }

    [Test]
    public async Task GetProposalByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockProposalRepository.Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Proposal?)null);

        // Act
        var result = await _proposalService.GetProposalByIdAsync(999);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task CreateProposalAsync_ShouldCreateProposal_WhenValidData()
    {
        // Arrange
        var newProposal = new Proposal
        {
            Id = 2,
            Title = "New Test Proposal",
            Description = "New test proposal description",
            CategoryId = 1,
            CreatedById = "test-user-1",
            Status = ProposalStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockMapper.Setup(x => x.Map<Proposal>(_createProposalDto))
            .Returns(newProposal);

        _mockProposalRepository.Setup(x => x.AddAsync(It.IsAny<Proposal>()))
            .ReturnsAsync(newProposal);

        var expectedDto = new ProposalDto
        {
            Id = 2,
            Title = "New Test Proposal",
            Description = "New test proposal description"
        };

        _mockMapper.Setup(x => x.Map<ProposalDto>(newProposal))
            .Returns(expectedDto);

        // Act
        var result = await _proposalService.CreateProposalAsync(_createProposalDto, "test-user-1");

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("New Test Proposal");
        result.Id.ShouldBe(2);

        _mockProposalRepository.Verify(x => x.AddAsync(It.IsAny<Proposal>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateProposalAsync_ShouldThrowArgumentException_WhenUserIdIsNull()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentException>(() => 
            _proposalService.CreateProposalAsync(_createProposalDto, null!));
    }

    [Test]
    public async Task CreateProposalAsync_ShouldThrowArgumentNullException_WhenDtoIsNull()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => 
            _proposalService.CreateProposalAsync(null!, "test-user-1"));
    }

    [Test]
    public async Task DeleteProposalAsync_ShouldDeleteProposal_WhenUserIsOwner()
    {
        // Arrange
        _mockProposalRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(_testProposal);

        // Act
        var result = await _proposalService.DeleteProposalAsync(1, "test-user-1");

        // Assert
        result.ShouldBeTrue();
        _mockProposalRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteProposalAsync_ShouldReturnFalse_WhenUserNotOwner()
    {
        // Arrange
        _mockProposalRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(_testProposal);

        // Act
        var result = await _proposalService.DeleteProposalAsync(1, "different-user");

        // Assert
        result.ShouldBeFalse();
        _mockProposalRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Test]
    public async Task IncrementViewsAsync_ShouldIncrementViews()
    {
        // Arrange & Act
        await _proposalService.IncrementViewsAsync(1);

        // Assert
        _mockProposalRepository.Verify(x => x.IncrementViewsAsync(1), Times.Once);
    }

    [Test]
    public async Task GetCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<Category> { _testCategory };
        var categoryDtos = new List<CategoryDto>
        {
            new CategoryDto
            {
                Id = 1,
                Name = "Fiscalité",
                Description = "Questions fiscales",
                Color = "#dc3545",
                IconClass = "fas fa-coins",
                ProposalsCount = 5
            }
        };

        _mockCategoryRepository.Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(categories);

        _mockMapper.Setup(x => x.Map<List<CategoryDto>>(categories))
            .Returns(categoryDtos);

        // Act
        var result = await _proposalService.GetCategoriesAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result.First().Name.ShouldBe("Fiscalité");

        _mockCategoryRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
    }
}