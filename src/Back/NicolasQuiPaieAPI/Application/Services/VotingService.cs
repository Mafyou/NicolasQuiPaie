namespace NicolasQuiPaieAPI.Application.Services;

/// <summary>
/// Enhanced voting service with C# 13.0 improvements and democratic voting (1 vote = 1 voice)
/// </summary>
public class VotingService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<VotingService> logger,
    IUserRepository userRepository) : IVotingService
{
    /// <summary>
    /// Casts a vote with democratic equality: 1 Nicolas = 1 voice regardless of contribution level
    /// </summary>
    public async Task<VoteDto> CastVoteAsync(CreateVoteDto voteDto, string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentNullException.ThrowIfNull(voteDto);

        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Get user for reputation updates and badge tracking
            var user = await userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Check if user already voted for this proposal
            var existingVote = await unitOfWork.Votes.GetUserVoteForProposalAsync(userId, voteDto.ProposalId);

            VoteDto resultVote;

            if (existingVote is not null)
            {
                // Update existing vote
                resultVote = await UpdateExistingVoteAsync(existingVote, voteDto, user);
            }
            else
            {
                // Create new vote
                resultVote = await CreateNewVoteAsync(voteDto, userId, user);
            }

            // Update user reputation and badge progression
            await UpdateUserReputationAsync(user, voteDto.VoteType);

            // ?? FIX: Save changes FIRST so the new vote is visible to the count query
            await unitOfWork.SaveChangesAsync();

            // Update proposal vote counts AFTER saving the vote
            await unitOfWork.Proposals.UpdateVoteCountsAsync(voteDto.ProposalId);

            // Save the vote count updates
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();

            logger.LogInformation("Democratic vote cast successfully for proposal {ProposalId} by user {UserId} with equal weight (1 voice)",
                voteDto.ProposalId, userId);

            return resultVote;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error casting vote for proposal {ProposalId} by user {UserId}",
                voteDto.ProposalId, userId);
            throw;
        }
    }

    /// <summary>
    /// Gets user's vote for a specific proposal
    /// </summary>
    public async Task<VoteDto?> GetUserVoteForProposalAsync(string userId, int proposalId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        try
        {
            var vote = await unitOfWork.Votes.GetUserVoteForProposalAsync(userId, proposalId);
            return vote?.ToVoteDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving vote for proposal {ProposalId} by user {UserId}",
                proposalId, userId);
            throw;
        }
    }

    /// <summary>
    /// Removes a user's vote from a proposal
    /// </summary>
    public async Task RemoveVoteAsync(string userId, int proposalId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var vote = await unitOfWork.Votes.GetUserVoteForProposalAsync(userId, proposalId);
            if (vote is not null)
            {
                await unitOfWork.Votes.DeleteAsync(vote.Id);

                // Optionally adjust user reputation for vote removal
                var user = await userRepository.GetByIdAsync(userId);
                if (user is not null)
                {
                    await AdjustReputationForVoteRemovalAsync(user, vote.VoteType);
                }

                // ?? FIX: Save changes FIRST so the deleted vote is no longer counted
                await unitOfWork.SaveChangesAsync();

                // Update proposal vote counts AFTER deleting the vote
                await unitOfWork.Proposals.UpdateVoteCountsAsync(proposalId);

                // Save the vote count updates
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Vote removed for proposal {ProposalId} by user {UserId}",
                    proposalId, userId);
            }
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync();
            logger.LogError(ex, "Error removing vote for proposal {ProposalId} by user {UserId}",
                proposalId, userId);
            throw;
        }
    }

    /// <summary>
    /// Gets all votes for a specific proposal
    /// </summary>
    public async Task<IReadOnlyList<VoteDto>> GetVotesForProposalAsync(int proposalId)
    {
        try
        {
            var votes = await unitOfWork.Votes.GetVotesForProposalAsync(proposalId);
            return votes.Select(v => v.ToVoteDto()).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving votes for proposal {ProposalId}", proposalId);
            throw;
        }
    }

    /// <summary>
    /// Gets all votes by a specific user
    /// </summary>
    public async Task<IReadOnlyList<VoteDto>> GetUserVotesAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        try
        {
            var votes = await unitOfWork.Votes.GetUserVotesAsync(userId);
            return votes.Select(v => v.ToVoteDto()).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving votes for user {UserId}", userId);
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// Updates an existing vote with new information
    /// </summary>
    private async Task<VoteDto> UpdateExistingVoteAsync(Vote existingVote, CreateVoteDto voteDto, ApplicationUser user)
    {
        existingVote.VoteType = (Infrastructure.Models.VoteType)(int)voteDto.VoteType;
        existingVote.Comment = voteDto.Comment;
        existingVote.VotedAt = DateTime.UtcNow;
        existingVote.Weight = 1; // Democratic equality: 1 Nicolas = 1 voice

        var updatedVote = await unitOfWork.Votes.UpdateAsync(existingVote);
        return updatedVote.ToVoteDto();
    }

    /// <summary>
    /// Creates a new vote
    /// </summary>
    private async Task<VoteDto> CreateNewVoteAsync(CreateVoteDto voteDto, string userId, ApplicationUser user)
    {
        var vote = new Vote
        {
            UserId = userId,
            ProposalId = voteDto.ProposalId,
            VoteType = (Infrastructure.Models.VoteType)(int)voteDto.VoteType,
            Comment = voteDto.Comment,
            VotedAt = DateTime.UtcNow,
            Weight = 1 // Democratic equality: 1 Nicolas = 1 voice, regardless of contribution level
        };

        var createdVote = await unitOfWork.Votes.AddAsync(vote);
        return createdVote.ToVoteDto();
    }

    /// <summary>
    /// Updates user reputation based on voting activity (for badge progression only)
    /// This affects the contribution level badges but NOT the vote weight
    /// </summary>
    private async Task UpdateUserReputationAsync(ApplicationUser user, NicolasQuiPaieData.DTOs.VoteType voteType)
    {
        // Base reputation points for voting
        const int baseVotingPoints = 1;

        // Bonus points for constructive voting (voting FOR proposals)
        int reputationChange = voteType switch
        {
            NicolasQuiPaieData.DTOs.VoteType.For => baseVotingPoints + 1, // Encourage positive voting
            NicolasQuiPaieData.DTOs.VoteType.Against => baseVotingPoints,
            _ => baseVotingPoints
        };

        user.ReputationScore += reputationChange;

        // Check for contribution level upgrade (badge progression only)
        await CheckAndUpdateContributionLevelAsync(user);

        await userRepository.UpdateAsync(user);
    }

    /// <summary>
    /// Adjusts reputation when a vote is removed (affects badges only)
    /// </summary>
    private async Task AdjustReputationForVoteRemovalAsync(ApplicationUser user, Infrastructure.Models.VoteType removedVoteType)
    {
        // Subtract reputation points for vote removal
        int reputationChange = removedVoteType switch
        {
            Infrastructure.Models.VoteType.For => -2, // Slight penalty for removing positive votes
            Infrastructure.Models.VoteType.Against => -1,
            _ => -1
        };

        user.ReputationScore = Math.Max(0, user.ReputationScore + reputationChange);
        await userRepository.UpdateAsync(user);
    }

    /// <summary>
    /// Checks and updates user's contribution level based on reputation (for badge display only)
    /// Note: This only affects the badge shown to the user, NOT their vote weight
    /// </summary>
    private static async Task CheckAndUpdateContributionLevelAsync(ApplicationUser user)
    {
        var newLevel = user.ReputationScore switch
        {
            >= 1000 => Infrastructure.Models.ContributionLevel.NicolasSupreme,
            >= 500 => Infrastructure.Models.ContributionLevel.GrosNicolas,
            >= 100 => Infrastructure.Models.ContributionLevel.GrosMoyenNicolas,
            _ => Infrastructure.Models.ContributionLevel.PetitNicolas
        };

        if (newLevel != user.ContributionLevel)
        {
            user.ContributionLevel = newLevel;
            // Could trigger badge notification here (badges are for recognition only)
        }

        await Task.CompletedTask;
    }

    #endregion
}

/// <summary>
/// Extension methods for mapping between Vote models and DTOs
/// </summary>
public static class VoteExtensions
{
    /// <summary>
    /// Converts a Vote entity to VoteDto
    /// </summary>
    public static VoteDto ToVoteDto(this Vote vote) => new()
    {
        Id = vote.Id,
        VoteType = (NicolasQuiPaieData.DTOs.VoteType)(int)vote.VoteType,
        VotedAt = vote.VotedAt,
        Weight = vote.Weight,
        Comment = vote.Comment,
        UserId = vote.UserId,
        ProposalId = vote.ProposalId,
        Proposal = vote.Proposal?.ToProposalDto()
    };

    /// <summary>
    /// Converts a Proposal entity to ProposalDto (basic mapping)
    /// </summary>
    public static ProposalDto ToProposalDto(this Proposal proposal) => new()
    {
        Id = proposal.Id,
        Title = proposal.Title,
        Description = proposal.Description,
        Status = (NicolasQuiPaieData.DTOs.ProposalStatus)(int)proposal.Status,
        CreatedAt = proposal.CreatedAt,
        UpdatedAt = proposal.UpdatedAt,
        ClosedAt = proposal.ClosedAt,
        VotesFor = proposal.VotesFor,
        VotesAgainst = proposal.VotesAgainst,
        ViewsCount = proposal.ViewsCount,
        IsFeatured = proposal.IsFeatured,
        ImageUrl = proposal.ImageUrl,
        Tags = proposal.Tags,
        CreatedById = proposal.CreatedById,
        CreatedByDisplayName = proposal.CreatedBy?.DisplayName ?? "",
        CategoryId = proposal.CategoryId,
        CategoryName = proposal.Category?.Name ?? "",
        CategoryColor = proposal.Category?.Color ?? "",
        CategoryIcon = proposal.Category?.IconClass ?? ""
    };
}