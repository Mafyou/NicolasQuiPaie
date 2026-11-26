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

    public async Task<IReadOnlyList<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetActiveProposalsAsync(skip, take, categoryId, search);
            return [.. proposals.Select(p => p.ToDto())];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions actives");
            throw;
        }
    }

    public async Task<IReadOnlyList<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetTrendingProposalsAsync(take);
            return [.. proposals.Select(p => p.ToDto())];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions tendances");
            throw;
        }
    }

    public async Task<IReadOnlyList<ProposalDto>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetRecentProposalsAsync(skip, take, category, search);
            return [.. proposals.Select(p => p.ToDto())];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions récentes");
            throw;
        }
    }

    public async Task<IReadOnlyList<ProposalDto>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetPopularProposalsAsync(skip, take, category, search);
            return [.. proposals.Select(p => p.ToDto())];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions populaires");
            throw;
        }
    }

    public async Task<IReadOnlyList<ProposalDto>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var proposals = await _unitOfWork.Proposals.GetControversialProposalsAsync(skip, take, category, search);
            return [.. proposals.Select(p => p.ToDto())];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions controversées");
            throw;
        }
    }

    public async Task<ProposalLookup> GetProposalByIdAsync(int id)
    {
        try
        {
            return await _unitOfWork.Proposals.GetByIdAsync(id) is { } proposal ? proposal.ToDto() : new ProposalNotFound(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la proposition {ProposalId}", id);
            throw;
        }
    }

    public async Task<ProposalCreation> CreateProposalAsync(CreateProposalDto createDto, string userId)
    {
        try
        {
            // Checked before saving: otherwise the foreign key refuses the row and the caller gets a 500
            if (!await _unitOfWork.Categories.ExistsAsync(createDto.CategoryId))
            {
                _logger.LogWarning("Proposition refusée: la catégorie {CategoryId} n'existe pas ({UserId})", createDto.CategoryId, userId);
                return new UnknownCategory(createDto.CategoryId);
            }

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

    public async Task<ProposalChange> UpdateProposalAsync(int id, UpdateProposalDto updateDto, string userId)
    {
        try
        {
            if (await _unitOfWork.Proposals.GetByIdAsync(id) is not { } proposal) return new ProposalNotFound(id);
            if (proposal.CreatedById != userId) return new NotProposalAuthor(id, userId);

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
    public async Task<ProposalChange> ToggleProposalStatusAsync(int proposalId, ProposalStatus newStatus, string userId)
    {
        try
        {
            if (await _unitOfWork.Proposals.GetByIdAsync(proposalId) is not { } proposal) return new ProposalNotFound(proposalId);

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

    public async Task<ProposalDeletion> DeleteProposalAsync(int id, string userId)
    {
        try
        {
            if (await _unitOfWork.Proposals.GetByIdAsync(id) is not { } proposal) return new ProposalNotFound(id);
            if (proposal.CreatedById != userId) return new NotProposalAuthor(id, userId);

            await _unitOfWork.Proposals.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Proposition supprimée: {ProposalId} par {UserId}", id, userId);
            return new ProposalDeleted(id);
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