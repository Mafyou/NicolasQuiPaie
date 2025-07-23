using AutoMapper;
using Microsoft.Extensions.Logging;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieAPI.Application.Services;
using NicolasQuiPaieData.DTOs;
using NicolasQuiPaieData.Models;

namespace NicolasQuiPaie.UnitTests.Services
{
    [TestFixture]
    public class ProposalServiceTests
    {
        private Mock<IUnitOfWork> _mockUnitOfWork;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<ProposalService>> _mockLogger;
        private Mock<IProposalRepository> _mockProposalRepository;
        private ProposalService _proposalService;

        [SetUp]
        public void Setup()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ProposalService>>();
            _mockProposalRepository = new Mock<IProposalRepository>();

            _mockUnitOfWork.Setup(x => x.Proposals).Returns(_mockProposalRepository.Object);

            _proposalService = new ProposalService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Test]
        public async Task GetActiveProposalsAsync_ShouldReturnProposals_WhenProposalsExist()
        {
            // Arrange
            var proposals = new List<Proposal>
            {
                new Proposal { Id = 1, Title = "Test Proposal 1", Status = ProposalStatus.Active },
                new Proposal { Id = 2, Title = "Test Proposal 2", Status = ProposalStatus.Active }
            };

            var proposalDtos = new List<ProposalDto>
            {
                new ProposalDto { Id = 1, Title = "Test Proposal 1", Status = ProposalStatus.Active },
                new ProposalDto { Id = 2, Title = "Test Proposal 2", Status = ProposalStatus.Active }
            };

            _mockProposalRepository
                .Setup(x => x.GetActiveProposalsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync(proposals);

            _mockMapper
                .Setup(x => x.Map<IEnumerable<ProposalDto>>(It.IsAny<IEnumerable<Proposal>>()))
                .Returns(proposalDtos);

            // Act
            var result = await _proposalService.GetActiveProposalsAsync();

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
            result.First().Title.ShouldBe("Test Proposal 1");
            
            _mockProposalRepository.Verify(x => x.GetActiveProposalsAsync(0, 20, null, null), Times.Once);
            _mockMapper.Verify(x => x.Map<IEnumerable<ProposalDto>>(proposals), Times.Once);
        }

        [Test]
        public async Task GetProposalByIdAsync_ShouldReturnProposal_WhenProposalExists()
        {
            // Arrange
            var proposalId = 1;
            var proposal = new Proposal { Id = proposalId, Title = "Test Proposal", Status = ProposalStatus.Active };
            var proposalDto = new ProposalDto { Id = proposalId, Title = "Test Proposal", Status = ProposalStatus.Active };

            _mockProposalRepository
                .Setup(x => x.GetByIdAsync(proposalId))
                .ReturnsAsync(proposal);

            _mockMapper
                .Setup(x => x.Map<ProposalDto>(proposal))
                .Returns(proposalDto);

            // Act
            var result = await _proposalService.GetProposalByIdAsync(proposalId);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(proposalId);
            result.Title.ShouldBe("Test Proposal");

            _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
            _mockMapper.Verify(x => x.Map<ProposalDto>(proposal), Times.Once);
        }

        [Test]
        public async Task GetProposalByIdAsync_ShouldReturnNull_WhenProposalDoesNotExist()
        {
            // Arrange
            var proposalId = 999;

            _mockProposalRepository
                .Setup(x => x.GetByIdAsync(proposalId))
                .ReturnsAsync((Proposal?)null);

            // Act
            var result = await _proposalService.GetProposalByIdAsync(proposalId);

            // Assert
            result.ShouldBeNull();

            _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
            _mockMapper.Verify(x => x.Map<ProposalDto>(It.IsAny<Proposal>()), Times.Never);
        }

        [Test]
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
                Status = ProposalStatus.Active
            };

            var createdProposal = new Proposal
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                CategoryId = createDto.CategoryId,
                CreatedById = userId,
                Status = ProposalStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            var proposalDto = new ProposalDto
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                CategoryId = createDto.CategoryId,
                CreatedById = userId,
                Status = ProposalStatus.Active
            };

            _mockMapper
                .Setup(x => x.Map<Proposal>(createDto))
                .Returns(proposal);

            _mockProposalRepository
                .Setup(x => x.AddAsync(It.IsAny<Proposal>()))
                .ReturnsAsync(createdProposal);

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(x => x.Map<ProposalDto>(createdProposal))
                .Returns(proposalDto);

            // Act
            var result = await _proposalService.CreateProposalAsync(createDto, userId);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(createDto.Title);
            result.CreatedById.ShouldBe(userId);
            result.Status.ShouldBe(ProposalStatus.Active);

            _mockProposalRepository.Verify(x => x.AddAsync(It.Is<Proposal>(p => 
                p.Title == createDto.Title && 
                p.CreatedById == userId && 
                p.Status == ProposalStatus.Active)), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
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
                Status = ProposalStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProposal = new Proposal
            {
                Id = proposalId,
                Title = updateDto.Title,
                Description = updateDto.Description,
                CategoryId = updateDto.CategoryId,
                CreatedById = userId,
                Status = ProposalStatus.Active,
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

            _mockMapper
                .Setup(x => x.Map(updateDto, existingProposal))
                .Callback<UpdateProposalDto, Proposal>((dto, proposal) =>
                {
                    proposal.Title = dto.Title;
                    proposal.Description = dto.Description;
                    proposal.CategoryId = dto.CategoryId;
                });

            _mockProposalRepository
                .Setup(x => x.UpdateAsync(It.IsAny<Proposal>()))
                .ReturnsAsync(updatedProposal);

            _mockUnitOfWork
                .Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            _mockMapper
                .Setup(x => x.Map<ProposalDto>(updatedProposal))
                .Returns(proposalDto);

            // Act
            var result = await _proposalService.UpdateProposalAsync(proposalId, updateDto, userId);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(updateDto.Title);

            _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
            _mockProposalRepository.Verify(x => x.UpdateAsync(It.IsAny<Proposal>()), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task UpdateProposalAsync_ShouldThrowUnauthorizedException_WhenUserIsNotOwner()
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
                Status = ProposalStatus.Active
            };

            _mockProposalRepository
                .Setup(x => x.GetByIdAsync(proposalId))
                .ReturnsAsync(existingProposal);

            // Act & Assert
            var exception = await Should.ThrowAsync<UnauthorizedAccessException>(
                () => _proposalService.UpdateProposalAsync(proposalId, updateDto, userId));

            exception.Message.ShouldBe("Vous n'êtes pas autorisé à modifier cette proposition");

            _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
            _mockProposalRepository.Verify(x => x.UpdateAsync(It.IsAny<Proposal>()), Times.Never);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteProposalAsync_ShouldDeleteProposal_WhenUserIsOwner()
        {
            // Arrange
            var proposalId = 1;
            var userId = "user123";

            var existingProposal = new Proposal
            {
                Id = proposalId,
                CreatedById = userId,
                Status = ProposalStatus.Active
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
            await _proposalService.DeleteProposalAsync(proposalId, userId);

            // Assert
            _mockProposalRepository.Verify(x => x.GetByIdAsync(proposalId), Times.Once);
            _mockProposalRepository.Verify(x => x.DeleteAsync(proposalId), Times.Once);
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CanUserEditProposalAsync_ShouldReturnTrue_WhenUserIsOwner()
        {
            // Arrange
            var proposalId = 1;
            var userId = "user123";

            var proposal = new Proposal
            {
                Id = proposalId,
                CreatedById = userId,
                Status = ProposalStatus.Active
            };

            _mockProposalRepository
                .Setup(x => x.GetByIdAsync(proposalId))
                .ReturnsAsync(proposal);

            // Act
            var result = await _proposalService.CanUserEditProposalAsync(proposalId, userId);

            // Assert
            result.ShouldBeTrue();
        }

        [Test]
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
                Status = ProposalStatus.Active
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
}