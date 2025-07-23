using NicolasQuiPaieData.DTOs;
using NicolasQuiPaieAPI.Infrastructure.Models;
using System.Security.Claims;

namespace NicolasQuiPaieAPI.Application.Interfaces
{
    public interface IProposalService
    {
        Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null);
        Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5);
        Task<ProposalDto?> GetProposalByIdAsync(int id);
        Task<ProposalDto> CreateProposalAsync(CreateProposalDto createDto, string userId);
        Task<ProposalDto> UpdateProposalAsync(int id, UpdateProposalDto updateDto, string userId);
        Task DeleteProposalAsync(int id, string userId);
        Task<bool> CanUserEditProposalAsync(int proposalId, string userId);
    }

    /// <summary>
    /// Enhanced voting service interface with proper return types and vote weighting
    /// </summary>
    public interface IVotingService
    {
        /// <summary>
        /// Casts a vote with proper weight calculation based on fiscal level
        /// </summary>
        Task<VoteDto> CastVoteAsync(CreateVoteDto voteDto, string userId);
        
        /// <summary>
        /// Gets user's vote for a specific proposal
        /// </summary>
        Task<VoteDto?> GetUserVoteForProposalAsync(string userId, int proposalId);
        
        /// <summary>
        /// Removes a user's vote from a proposal
        /// </summary>
        Task RemoveVoteAsync(string userId, int proposalId);
        
        /// <summary>
        /// Gets all votes for a specific proposal
        /// </summary>
        Task<IReadOnlyList<VoteDto>> GetVotesForProposalAsync(int proposalId);
        
        /// <summary>
        /// Gets all votes by a specific user
        /// </summary>
        Task<IReadOnlyList<VoteDto>> GetUserVotesAsync(string userId);
    }

    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId);
        Task<CommentDto> CreateCommentAsync(CreateCommentDto commentDto, string userId);
        Task<CommentDto> UpdateCommentAsync(int id, string content, string userId);
        Task DeleteCommentAsync(int id, string userId);
        Task<CommentDto> LikeCommentAsync(int commentId, string userId);
        Task UnlikeCommentAsync(int commentId, string userId);
    }

    public interface IAnalyticsService
    {
        Task<GlobalStatsDto> GetGlobalStatsAsync();
        Task<VotingTrendsDto> GetVotingTrendsAsync(int days = 30);
        Task<FiscalLevelDistributionDto> GetFiscalLevelDistributionAsync();
        Task<TopContributorsDto> GetTopContributorsAsync(int take = 10);
        Task<RecentActivityDto> GetRecentActivityAsync(int take = 20);
        Task<FrustrationBarometerDto> GetFrustrationBarometerAsync();
    }

    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<UserStatsDto> GetUserStatsAsync(string userId);
        Task UpdateUserReputationAsync(string userId, int points);
        Task<bool> CanUserPerformActionAsync(string userId, string action);
    }

    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        bool IsTokenValid(string token);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task SendWelcomeEmailAsync(string email, string displayName);
    }
}