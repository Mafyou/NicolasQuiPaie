using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieWeb.Data;
using NicolasQuiPaieWeb.Data.Models;

namespace NicolasQuiPaieWeb.Services
{
    public class VotingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VotingService> _logger;

        public VotingService(ApplicationDbContext context, ILogger<VotingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> CastVoteAsync(string userId, int proposalId, VoteType voteType)
        {
            try
            {
                // Get user 
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Check if user already voted
                var existingVote = await _context.Votes
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.ProposalId == proposalId);

                var proposal = await _context.Proposals.FindAsync(proposalId);
                if (proposal == null) return false;

                if (existingVote != null)
                {
                    // Update existing vote - remove old vote and add new vote
                    RemoveVoteFromProposal(proposal, existingVote.VoteType);
                    
                    existingVote.VoteType = voteType;
                    existingVote.VotedAt = DateTime.UtcNow;
                    // Weight is always 1 per vote
                    existingVote.Weight = 1;
                    
                    AddVoteToProposal(proposal, voteType);
                }
                else
                {
                    // Create new vote - each vote has weight 1
                    var newVote = new Vote
                    {
                        UserId = userId,
                        ProposalId = proposalId,
                        VoteType = voteType,
                        Weight = 1, // Simplified: 1 vote = 1 weight
                        VotedAt = DateTime.UtcNow
                    };

                    _context.Votes.Add(newVote);
                    AddVoteToProposal(proposal, voteType);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error casting vote for user {UserId} on proposal {ProposalId}", userId, proposalId);
                return false;
            }
        }

        public async Task<Vote?> GetUserVoteAsync(string userId, int proposalId)
        {
            return await _context.Votes
                .Include(v => v.Proposal)
                .FirstOrDefaultAsync(v => v.UserId == userId && v.ProposalId == proposalId);
        }

        public async Task<bool> CanUserVoteAsync(string userId, int proposalId)
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal == null || proposal.Status != ProposalStatus.Active)
                return false;

            // Users can always vote (but only once per proposal)
            return true;
        }

        private void AddVoteToProposal(Proposal proposal, VoteType voteType)
        {
            // Simple counting: each vote counts as 1
            switch (voteType)
            {
                case VoteType.For:
                    proposal.VotesFor += 1;
                    break;
                case VoteType.Against:
                    proposal.VotesAgainst += 1;
                    break;
            }
        }

        private void RemoveVoteFromProposal(Proposal proposal, VoteType voteType)
        {
            // Simple counting: each vote counts as 1
            switch (voteType)
            {
                case VoteType.For:
                    proposal.VotesFor = Math.Max(0, proposal.VotesFor - 1);
                    break;
                case VoteType.Against:
                    proposal.VotesAgainst = Math.Max(0, proposal.VotesAgainst - 1);
                    break;
            }
        }

        public async Task<Dictionary<string, object>> GetVotingStatsAsync(int proposalId)
        {
            var votes = await _context.Votes
                .Include(v => v.User)
                .Where(v => v.ProposalId == proposalId)
                .ToListAsync();

            var stats = new Dictionary<string, object>
            {
                ["TotalVotes"] = votes.Count,
                ["VotesFor"] = votes.Count(v => v.VoteType == VoteType.For),
                ["VotesAgainst"] = votes.Count(v => v.VoteType == VoteType.Against),
                ["ContributionByLevel"] = votes.GroupBy(v => v.User.FiscalLevel)
                    .ToDictionary(g => g.Key.ToString(), g => new
                    {
                        Count = g.Count(),
                        For = g.Count(v => v.VoteType == VoteType.For),
                        Against = g.Count(v => v.VoteType == VoteType.Against),
                        // Badge information for contribution tracking
                        Badge = GetBadgeInfo(g.Key)
                    })
            };

            return stats;
        }

        private object GetBadgeInfo(FiscalLevel level)
        {
            return level switch
            {
                FiscalLevel.PetitNicolas => new { Name = "?? Petit Nicolas", Description = "Contributeur débutant" },
                FiscalLevel.GrosNicolas => new { Name = "?? Gros Nicolas", Description = "Contributeur actif" },
                FiscalLevel.NicolasSupreme => new { Name = "?? Nicolas Suprême", Description = "Contributeur expert" },
                _ => new { Name = "?? Petit Nicolas", Description = "Contributeur débutant" }
            };
        }
    }
}