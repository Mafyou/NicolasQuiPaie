using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieWebApp.Services
{
    public interface IProposalApiService
    {
        Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null);
        Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5);
        Task<ProposalDto?> GetProposalByIdAsync(int id);
        Task<ProposalDto> CreateProposalAsync(CreateProposalDto createDto);
        Task<ProposalDto> UpdateProposalAsync(int id, UpdateProposalDto updateDto);
        Task DeleteProposalAsync(int id);
        Task<bool> CanUserEditProposalAsync(int proposalId);
    }

    public interface IVotingApiService
    {
        Task<VoteDto> CastVoteAsync(CreateVoteDto voteDto);
        Task<VoteDto?> GetUserVoteForProposalAsync(int proposalId);
        Task RemoveVoteAsync(int proposalId);
        Task<IEnumerable<VoteDto>> GetVotesForProposalAsync(int proposalId);
        Task<IEnumerable<VoteDto>> GetUserVotesAsync();
    }

    public interface ICommentApiService
    {
        Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId);
        Task<CommentDto> CreateCommentAsync(CreateCommentDto commentDto);
        Task<CommentDto> UpdateCommentAsync(int id, string content);
        Task DeleteCommentAsync(int id);
        Task<CommentDto> LikeCommentAsync(int commentId);
        Task UnlikeCommentAsync(int commentId);
    }

    public interface IAnalyticsApiService
    {
        Task<GlobalStatsDto> GetGlobalStatsAsync();
        Task<VotingTrendsDto> GetVotingTrendsAsync(int days = 30);
        Task<FiscalLevelDistributionDto> GetFiscalLevelDistributionAsync();
        Task<TopContributorsDto> GetTopContributorsAsync(int take = 10);
        Task<RecentActivityDto> GetRecentActivityAsync(int take = 20);
        Task<FrustrationBarometerDto> GetFrustrationBarometerAsync();
    }

    public interface IUserApiService
    {
        Task<UserDto?> GetCurrentUserAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto> UpdateUserProfileAsync(UpdateUserProfileDto updateDto);
        Task<UserStatsDto> GetUserStatsAsync(string userId);
    }

    public interface ISignalRService
    {
        Task StartConnectionAsync();
        Task StopConnectionAsync();
        Task JoinProposalGroupAsync(int proposalId);
        Task LeaveProposalGroupAsync(int proposalId);
        event Action<int, VoteDto>? OnVoteUpdated;
        event Action<int, CommentDto>? OnCommentAdded;
        event Action<GlobalStatsDto>? OnGlobalStatsUpdated;
    }
}