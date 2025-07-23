using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieData.DTOs;
using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieAPI.Infrastructure.Data;

namespace NicolasQuiPaieAPI.Application.Services;

public class AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger) : IAnalyticsService
{
    public async Task<GlobalStatsDto> GetGlobalStatsAsync()
    {
        try
        {
            var totalUsers = await context.Users.CountAsync();
            var totalProposals = await context.Proposals.CountAsync();
            var totalVotes = await context.Votes.CountAsync();
            var totalComments = await context.Comments.Where(c => !c.IsDeleted).CountAsync();

            var activeProposals = await context.Proposals
                .Where(p => p.Status == Infrastructure.Models.ProposalStatus.Active)
                .CountAsync();

            return new GlobalStatsDto
            {
                TotalUsers = totalUsers,
                TotalProposals = totalProposals,
                TotalVotes = totalVotes,
                TotalComments = totalComments,
                ActiveProposals = activeProposals
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting global stats");
            return new GlobalStatsDto();
        }
    }

    public async Task<VotingTrendsDto> GetVotingTrendsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            var dailyVotes = await context.Votes
                .Where(v => v.VotedAt >= startDate)
                .GroupBy(v => v.VotedAt.Date)
                .Select(g => new DailyVoteCount
                {
                    Date = g.Key,
                    VotesFor = g.Count(v => v.VoteType == Infrastructure.Models.VoteType.For),
                    VotesAgainst = g.Count(v => v.VoteType == Infrastructure.Models.VoteType.Against)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            return new VotingTrendsDto
            {
                DailyVotes = dailyVotes
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting voting trends");
            return new VotingTrendsDto { DailyVotes = [] };
        }
    }

    public async Task<FiscalLevelDistributionDto> GetFiscalLevelDistributionAsync()
    {
        try
        {
            var distribution = await context.Users
                .GroupBy(u => u.FiscalLevel)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            var totalUsers = distribution.Sum(d => d.Count);
            var fiscalLevelCounts = distribution.Select(d => new FiscalLevelCount
            {
                LevelName = d.Level.ToString(),
                UserCount = d.Count,
                Percentage = totalUsers > 0 ? (double)d.Count / totalUsers * 100 : 0
            }).ToList();

            return new FiscalLevelDistributionDto
            {
                Distribution = fiscalLevelCounts
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting fiscal level distribution");
            return new FiscalLevelDistributionDto { Distribution = [] };
        }
    }

    public async Task<TopContributorsDto> GetTopContributorsAsync(int take = 10)
    {
        try
        {
            var contributors = await context.Users
                .OrderByDescending(u => u.ReputationScore)
                .Take(take)
                .Select(u => new UserContributionDto
                {
                    UserId = u.Id,
                    UserDisplayName = u.DisplayName ?? "Anonymous",
                    UserFiscalLevel = (NicolasQuiPaieData.DTOs.FiscalLevel)(int)u.FiscalLevel,
                    ContributionCount = context.Proposals.Count(p => p.CreatedById == u.Id) + 
                                      context.Votes.Count(v => v.UserId == u.Id) + 
                                      context.Comments.Count(c => c.UserId == u.Id && !c.IsDeleted),
                    ReputationScore = u.ReputationScore
                })
                .ToListAsync();

            return new TopContributorsDto
            {
                TopProposers = contributors.OrderByDescending(c => context.Proposals.Count(p => p.CreatedById == c.UserId)).Take(take).ToList(),
                TopVoters = contributors.OrderByDescending(c => context.Votes.Count(v => v.UserId == c.UserId)).Take(take).ToList(),
                TopCommenters = contributors.OrderByDescending(c => context.Comments.Count(c2 => c2.UserId == c.UserId && !c2.IsDeleted)).Take(take).ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting top contributors");
            return new TopContributorsDto();
        }
    }

    public async Task<RecentActivityDto> GetRecentActivityAsync(int take = 20)
    {
        try
        {
            // Get recent proposals
            var recentProposals = await context.Proposals
                .Include(p => p.CreatedBy)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take / 2)
                .Select(p => new RecentActivityItem
                {
                    Type = "Proposal",
                    Description = $"a créé la proposition '{p.Title}'",
                    UserId = p.CreatedById,
                    UserDisplayName = p.CreatedBy.DisplayName ?? "Anonymous",
                    Timestamp = p.CreatedAt,
                    RelatedItemId = p.Id.ToString(),
                    RelatedItemTitle = p.Title
                })
                .ToListAsync();

            // Get recent votes
            var recentVotes = await context.Votes
                .Include(v => v.User)
                .Include(v => v.Proposal)
                .OrderByDescending(v => v.VotedAt)
                .Take(take / 2)
                .Select(v => new RecentActivityItem
                {
                    Type = "Vote",
                    Description = $"a voté {(v.VoteType == Infrastructure.Models.VoteType.For ? "POUR" : "CONTRE")} '{v.Proposal.Title}'",
                    UserId = v.UserId,
                    UserDisplayName = v.User.DisplayName ?? "Anonymous",
                    Timestamp = v.VotedAt,
                    RelatedItemId = v.ProposalId.ToString(),
                    RelatedItemTitle = v.Proposal.Title
                })
                .ToListAsync();

            var allActivities = recentProposals.Concat(recentVotes)
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .ToList();

            return new RecentActivityDto
            {
                Activities = allActivities
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting recent activity");
            return new RecentActivityDto { Activities = [] };
        }
    }

    public async Task<FrustrationBarometerDto> GetFrustrationBarometerAsync()
    {
        try
        {
            // Calculate frustration based on voting patterns
            var totalVotes = await context.Votes.CountAsync();
            var againstVotes = await context.Votes.CountAsync(v => v.VoteType == Infrastructure.Models.VoteType.Against);
            
            var frustrationLevel = totalVotes > 0 ? (double)againstVotes / totalVotes * 100 : 0;

            // Determine mood based on frustration level
            string mood = frustrationLevel switch
            {
                < 30 => "Calm",
                < 50 => "Concerned",
                < 70 => "Frustrated",
                _ => "Angry"
            };

            return new FrustrationBarometerDto
            {
                FrustrationLevel = frustrationLevel,
                TotalVotes = totalVotes,
                TotalVotesAgainst = againstVotes,
                CurrentMood = mood,
                CategoryBreakdown = []
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting frustration barometer");
            return new FrustrationBarometerDto();
        }
    }
}