namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Client-side wrapper service for API calls - replaces the old database-dependent VotingService
/// </summary>
public class VotingService(ApiVotingService apiVotingService, ILogger<VotingService> logger)
{
    private readonly ApiVotingService _apiVotingService = apiVotingService;
    private readonly ILogger<VotingService> _logger = logger;

    public async Task<bool> CastVoteAsync(string userId, int proposalId, VoteType voteType)
    {
        try
        {
            var voteDto = new CreateVoteDto
            {
                ProposalId = proposalId,
                VoteType = voteType,
                Comment = null
            };

            return await _apiVotingService.CastVoteAsync(voteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error casting vote for user {UserId} on proposal {ProposalId}", userId, proposalId);
            return false;
        }
    }

    public async Task<VoteDto?> GetUserVoteAsync(string userId, int proposalId)
    {
        try
        {
            var votes = await _apiVotingService.GetUserVotesAsync(userId);
            return votes.FirstOrDefault(v => v.ProposalId == proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user vote for proposal {ProposalId}", proposalId);
            return null;
        }
    }
}