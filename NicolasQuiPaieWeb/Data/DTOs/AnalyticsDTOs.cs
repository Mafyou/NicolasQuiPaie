using NicolasQuiPaieWeb.Data.Models;

namespace NicolasQuiPaieWeb.Data.DTOs
{
    /// <summary>
    /// DTO pour les statistiques du tableau de bord
    /// </summary>
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalProposals { get; set; }
        public int ActiveProposals { get; set; }
        public int TotalVotes { get; set; }
        public int TotalComments { get; set; }
        public double RasLebolMeter { get; set; }
        public List<CategoryStatsDto> TopCategories { get; set; } = new();
        public List<DailyVoteStatsDto> DailyVoteTrends { get; set; } = new();
        public List<NicolasLevelStatsDto> NicolasLevelDistribution { get; set; } = new();
    }

    /// <summary>
    /// DTO pour les statistiques par catégorie
    /// </summary>
    public class CategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string CategoryColor { get; set; } = "";
        public string CategoryIcon { get; set; } = "";
        public int ProposalCount { get; set; }
        public int VoteCount { get; set; }
    }

    /// <summary>
    /// DTO pour les statistiques de votes quotidiens
    /// </summary>
    public class DailyVoteStatsDto
    {
        public DateTime Date { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public int TotalVotes => VotesFor + VotesAgainst;
    }

    /// <summary>
    /// DTO pour la distribution des niveaux Nicolas
    /// </summary>
    public class NicolasLevelStatsDto
    {
        public FiscalLevel Level { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// DTO pour les propositions tendances
    /// </summary>
    public class TrendingProposalDto
    {
        public ProposalDto Proposal { get; set; } = new();
        public int RecentVotes { get; set; }
        public int RecentComments { get; set; }
        public int TrendScore { get; set; }
    }

    /// <summary>
    /// DTO pour les top contributeurs
    /// </summary>
    public class TopContributorDto
    {
        public string UserId { get; set; } = "";
        public string UserDisplayName { get; set; } = "";
        public FiscalLevel UserFiscalLevel { get; set; }
        public int ProposalsCount { get; set; }
        public int VotesCount { get; set; }
        public int CommentsCount { get; set; }
        public int TotalScore { get; set; }
    }
}