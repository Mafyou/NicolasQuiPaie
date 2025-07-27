namespace NicolasQuiPaieAPI.Application.Services;

public class ProposalService : IProposalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProposalService> _logger;

    public ProposalService(IUnitOfWork unitOfWork, ILogger<ProposalService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetActiveProposalsAsync(skip, take, categoryId, search);
            return proposals.Select(p => p.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions actives");
            throw;
        }
    }

    public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetTrendingProposalsAsync(take);
            return proposals.Select(p => p.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions tendances");
            throw;
        }
    }

    public async Task<IEnumerable<ProposalDto>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetRecentProposalsAsync(skip, take, category, search);
            return proposals.Select(p => p.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions récentes");
            throw;
        }
    }

    public async Task<IEnumerable<ProposalDto>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetPopularProposalsAsync(skip, take, category, search);
            return proposals.Select(p => p.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions populaires");
            throw;
        }
    }

    public async Task<IEnumerable<ProposalDto>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetControversialProposalsAsync(skip, take, category, search);
            return proposals.Select(p => p.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions controversées");
            throw;
        }
    }

    public async Task<ProposalDto?> GetProposalByIdAsync(int id)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(id);
            return proposal?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la proposition {ProposalId}", id);
            throw;
        }
    }

    public async Task<ProposalDto> CreateProposalAsync(CreateProposalDto createDto, string userId)
    {
        try
        {
            var proposal = createDto.ToEntity();
            proposal.CreatedById = userId;
            proposal.Status = ProposalStatus.Active;
            proposal.CreatedAt = DateTime.UtcNow;

            var createdProposal = await _unitOfWork.Proposals.AddAsync(proposal);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Proposition créée avec succès: {ProposalId} par {UserId}", createdProposal.Id, userId);
            return createdProposal.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la proposition par {UserId}", userId);
            throw;
        }
    }

    public async Task<ProposalDto> UpdateProposalAsync(int id, UpdateProposalDto updateDto, string userId)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(id) ?? throw new ArgumentException($"Proposition {id} non trouvée");
            if (proposal.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à modifier cette proposition");
            }

            // Update properties manually instead of using AutoMapper
            proposal.Title = updateDto.Title;
            proposal.Description = updateDto.Description;
            proposal.CategoryId = updateDto.CategoryId;
            proposal.ImageUrl = updateDto.ImageUrl;
            proposal.Tags = updateDto.Tags;
            proposal.UpdatedAt = DateTime.UtcNow;

            var updatedProposal = await _unitOfWork.Proposals.UpdateAsync(proposal);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Proposition mise à jour: {ProposalId} par {UserId}", id, userId);
            return updatedProposal.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la proposition {ProposalId} par {UserId}", id, userId);
            throw;
        }
    }

    /// <summary>
    /// C# 13.0 - Toggle proposal status (SuperUser/Admin only) with modern pattern matching
    /// </summary>
    public async Task<ProposalDto> ToggleProposalStatusAsync(int proposalId, ProposalStatus newStatus, string userId)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(proposalId) ?? throw new ArgumentException($"Proposition {proposalId} non trouvée");

            var oldStatus = proposal.Status;
            proposal.Status = newStatus;
            proposal.UpdatedAt = DateTime.UtcNow;

            // Set ClosedAt when closing a proposal
            if (newStatus == ProposalStatus.Closed && proposal.ClosedAt == null)
            {
                proposal.ClosedAt = DateTime.UtcNow;
            }

            var updatedProposal = await _unitOfWork.Proposals.UpdateAsync(proposal);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Proposition status changed: {ProposalId} from {OldStatus} to {NewStatus} by SuperUser/Admin {UserId}",
                proposalId, oldStatus, newStatus, userId);

            return updatedProposal.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut de la proposition {ProposalId} par {UserId}", proposalId, userId);
            throw;
        }
    }

    public async Task DeleteProposalAsync(int id, string userId)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(id) ?? throw new ArgumentException($"Proposition {id} non trouvée");
            if (proposal.CreatedById != userId)
            {
                throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cette proposition");
            }

            await _unitOfWork.Proposals.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Proposition supprimée: {ProposalId} par {UserId}", id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la proposition {ProposalId} par {UserId}", id, userId);
            throw;
        }
    }

    public async Task<bool> CanUserEditProposalAsync(int proposalId, string userId)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(proposalId);
            return proposal is not null && proposal.CreatedById == userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification des droits d'édition pour la proposition {ProposalId} par {UserId}", proposalId, userId);
            return false;
        }
    }

    public async Task IncrementViewsAsync(int proposalId)
    {
        try
        {
            var proposal = await _unitOfWork.Proposals.GetByIdAsync(proposalId);
            if (proposal is not null)
            {
                proposal.ViewsCount++;
                await _unitOfWork.Proposals.UpdateAsync(proposal);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogDebug("Views count incremented for proposal {ProposalId}. New count: {ViewsCount}",
                    proposalId, proposal.ViewsCount);
            }
            else
            {
                _logger.LogWarning("Attempted to increment views for non-existent proposal {ProposalId}", proposalId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'incrémentation des vues pour la proposition {ProposalId}", proposalId);
            // Don't throw here as view counting is not critical functionality
        }
    }
}