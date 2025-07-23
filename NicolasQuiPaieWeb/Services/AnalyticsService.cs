using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using NicolasQuiPaieWeb.Data;
using NicolasQuiPaieWeb.Data.Models;
using NicolasQuiPaieWeb.Data.DTOs;

namespace NicolasQuiPaieWeb.Services
{
    public class AnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                var stats = new DashboardStatsDto();

                // ?? SOLUTION HYBRIDE: Essayer DbContext d'abord, fallback sur UserManager
                try
                {
                    stats.TotalUsers = await _context.Users.CountAsync();
                    _logger.LogInformation($"? TotalUsers depuis DbContext: {stats.TotalUsers}");
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, "?? Échec DbContext.Users.CountAsync(), utilisation de UserManager");
                    stats.TotalUsers = _userManager.Users.Count();
                    _logger.LogInformation($"?? TotalUsers depuis UserManager (fallback): {stats.TotalUsers}");
                }

                // Si toujours 0, diagnostic approfondi
                if (stats.TotalUsers == 0)
                {
                    var userManagerCount = _userManager.Users.Count();
                    _logger.LogWarning($"?? TotalUsers = 0 ! UserManager.Users.Count() = {userManagerCount}");
                    
                    if (userManagerCount > 0)
                    {
                        // Force l'utilisation de UserManager si DbContext est vide
                        stats.TotalUsers = userManagerCount;
                        _logger.LogInformation($"?? Utilisation forcée de UserManager: {stats.TotalUsers}");
                    }
                }

                // Basic counts avec gestion d'erreur
                try
                {
                    stats.TotalProposals = await _context.Proposals.CountAsync();
                    stats.TotalVotes = await _context.Votes.CountAsync();
                    stats.TotalComments = await _context.Comments.Where(c => !c.IsDeleted).CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors du comptage des propositions/votes/commentaires");
                    stats.TotalProposals = 0;
                    stats.TotalVotes = 0;
                    stats.TotalComments = 0;
                }

                // Active stats (last 30 days) avec gestion hybride
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                try
                {
                    stats.ActiveUsers = await _context.Users
                        .Where(u => u.Votes.Any(v => v.VotedAt >= thirtyDaysAgo) ||
                                   u.Comments.Any(c => c.CreatedAt >= thirtyDaysAgo))
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Échec calcul ActiveUsers depuis DbContext");
                    // Fallback approximatif basé sur UserManager
                    stats.ActiveUsers = Math.Min(stats.TotalUsers, 1);
                }

                try
                {
                    stats.ActiveProposals = await _context.Proposals
                        .Where(p => p.Status == ProposalStatus.Active)
                        .CountAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur calcul ActiveProposals");
                    stats.ActiveProposals = 0;
                }

                // Calculate Ras-le-bol meter (percentage of "Against" votes)
                try
                {
                    var totalVotesCount = await _context.Votes.CountAsync();
                    if (totalVotesCount > 0)
                    {
                        var againstVotes = await _context.Votes.Where(v => v.VoteType == VoteType.Against).CountAsync();
                        stats.RasLebolMeter = (double)againstVotes / totalVotesCount * 100;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur calcul RasLebolMeter");
                    stats.RasLebolMeter = 0;
                }

                // Top categories by activity
                try
                {
                    stats.TopCategories = await _context.Categories
                        .Select(c => new CategoryStatsDto
                        {
                            CategoryId = c.Id,
                            CategoryName = c.Name,
                            CategoryColor = c.Color,
                            CategoryIcon = c.IconClass,
                            ProposalCount = c.Proposals.Count(),
                            VoteCount = c.Proposals.SelectMany(p => p.Votes).Count()
                        })
                        .OrderByDescending(cs => cs.VoteCount)
                        .Take(5)
                        .ToListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur chargement TopCategories");
                    stats.TopCategories = new List<CategoryStatsDto>();
                }

                // Daily vote trends (last 7 days)
                try
                {
                    stats.DailyVoteTrends = await GetDailyVoteTrends(7);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur chargement DailyVoteTrends");
                    stats.DailyVoteTrends = new List<DailyVoteStatsDto>();
                }

                // Nicolas level distribution avec gestion hybride
                try
                {
                    stats.NicolasLevelDistribution = await GetNicolasLevelDistribution();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur chargement NicolasLevelDistribution");
                    stats.NicolasLevelDistribution = new List<NicolasLevelStatsDto>();
                }

                _logger.LogInformation($"?? Dashboard stats générées: Users={stats.TotalUsers}, Proposals={stats.TotalProposals}, Votes={stats.TotalVotes}");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                
                // Retourner au moins les utilisateurs depuis UserManager en cas d'erreur complète
                var fallbackStats = new DashboardStatsDto();
                try
                {
                    fallbackStats.TotalUsers = _userManager.Users.Count();
                    _logger.LogInformation($"?? Stats fallback activées: TotalUsers = {fallbackStats.TotalUsers}");
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Échec complet du fallback UserManager");
                }
                
                return fallbackStats;
            }
        }

        public async Task<List<DailyVoteStatsDto>> GetDailyVoteTrends(int days)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            var trends = new List<DailyVoteStatsDto>();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var nextDate = date.AddDays(1);

                var dayStats = new DailyVoteStatsDto
                {
                    Date = date,
                    VotesFor = await _context.Votes
                        .Where(v => v.VotedAt >= date && v.VotedAt < nextDate && v.VoteType == VoteType.For)
                        .CountAsync(),
                    VotesAgainst = await _context.Votes
                        .Where(v => v.VotedAt >= date && v.VotedAt < nextDate && v.VoteType == VoteType.Against)
                        .CountAsync()
                };

                trends.Add(dayStats);
            }

            return trends;
        }

        public async Task<List<NicolasLevelStatsDto>> GetNicolasLevelDistribution()
        {
            // ?? Stratégie hybride pour NicolasLevelDistribution
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                if (totalUsers == 0)
                {
                    // Fallback avec UserManager
                    var userManagerUsers = _userManager.Users.ToList();
                    var fallbackDistribution = userManagerUsers
                        .GroupBy(u => u.FiscalLevel)
                        .Select(g => new NicolasLevelStatsDto
                        {
                            Level = g.Key,
                            Count = g.Count(),
                            Percentage = userManagerUsers.Count > 0 ? (double)g.Count() / userManagerUsers.Count * 100 : 0
                        })
                        .ToList();
                    
                    _logger.LogInformation($"?? NicolasLevelDistribution depuis UserManager: {fallbackDistribution.Count} niveaux");
                    return fallbackDistribution;
                }

                return await _context.Users
                    .GroupBy(u => u.FiscalLevel)
                    .Select(g => new NicolasLevelStatsDto
                    {
                        Level = g.Key,
                        Count = g.Count(),
                        Percentage = (double)g.Count() / _context.Users.Count() * 100
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans GetNicolasLevelDistribution");
                return new List<NicolasLevelStatsDto>();
            }
        }

        public async Task<List<TrendingProposalDto>> GetTrendingProposalsAsync(int hours = 24)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            return await _context.Proposals
                .Include(p => p.Category)
                .Include(p => p.CreatedBy)
                .Where(p => p.Status == ProposalStatus.Active)
                .Select(p => new TrendingProposalDto
                {
                    Proposal = new ProposalDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        VotesFor = p.VotesFor,
                        VotesAgainst = p.VotesAgainst,
                        ViewsCount = p.ViewsCount,
                        IsFeatured = p.IsFeatured,
                        ClosedAt = p.ClosedAt,
                        CreatedById = p.CreatedById,
                        CreatedByDisplayName = p.CreatedBy.DisplayName,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        CategoryColor = p.Category.Color,
                        CategoryIcon = p.Category.IconClass
                    },
                    RecentVotes = p.Votes.Count(v => v.VotedAt >= cutoffTime),
                    RecentComments = p.Comments.Count(c => c.CreatedAt >= cutoffTime && !c.IsDeleted),
                    TrendScore = (p.Votes.Count(v => v.VotedAt >= cutoffTime) * 2) + 
                                p.Comments.Count(c => c.CreatedAt >= cutoffTime && !c.IsDeleted)
                })
                .OrderByDescending(tp => tp.TrendScore)
                .Take(10)
                .ToListAsync();
        }

        public async Task<List<TopContributorDto>> GetTopContributorsAsync(int count = 10)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            // ?? Stratégie hybride pour TopContributors
            try
            {
                return await _context.Users
                    .Select(u => new TopContributorDto
                    {
                        UserId = u.Id,
                        UserDisplayName = u.DisplayName ?? u.UserName ?? "Nicolas Anonyme",
                        UserFiscalLevel = u.FiscalLevel,
                        ProposalsCount = u.CreatedProposals.Count(),
                        VotesCount = u.Votes.Count(v => v.VotedAt >= thirtyDaysAgo),
                        CommentsCount = u.Comments.Count(c => c.CreatedAt >= thirtyDaysAgo && !c.IsDeleted),
                        TotalScore = u.CreatedProposals.Count() * 5 + 
                                    u.Votes.Count(v => v.VotedAt >= thirtyDaysAgo) + 
                                    u.Comments.Count(c => c.CreatedAt >= thirtyDaysAgo && !c.IsDeleted) * 2
                    })
                    .OrderByDescending(tc => tc.TotalScore)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur GetTopContributorsAsync, tentative fallback");
                
                // Fallback avec UserManager (contribution basique)
                try
                {
                    var users = _userManager.Users.ToList();
                    return users.Take(count).Select(u => new TopContributorDto
                    {
                        UserId = u.Id,
                        UserDisplayName = u.DisplayName ?? u.UserName ?? "Nicolas Anonyme",
                        UserFiscalLevel = u.FiscalLevel,
                        ProposalsCount = 0, // Pas de données disponibles en fallback
                        VotesCount = 0,
                        CommentsCount = 0,
                        TotalScore = 1 // Score minimal pour affichage
                    }).ToList();
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Échec complet GetTopContributorsAsync");
                    return new List<TopContributorDto>();
                }
            }
        }
    }
}