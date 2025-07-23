namespace NicolasQuiPaieData.DTOs;

/// <summary>
/// DTO pour les statistiques globales de la plateforme (record for immutability)
/// </summary>
public record GlobalStatsDto
{
    public int TotalUsers { get; init; }
    public int TotalProposals { get; init; }
    public int TotalVotes { get; init; }
    public int TotalComments { get; init; }
    public int ActiveProposals { get; init; }
    public double AverageParticipationRate { get; init; }
}

/// <summary>
/// DTO pour les statistiques du tableau de bord (record for immutability)
/// </summary>
public record DashboardStatsDto
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int TotalProposals { get; init; }
    public int ActiveProposals { get; init; }
    public int TotalVotes { get; init; }
    public int TotalComments { get; init; }
    public double RasLebolMeter { get; init; }
    public IReadOnlyList<CategoryStatsDto> TopCategories { get; init; } = [];
    public IReadOnlyList<DailyVoteStatsDto> DailyVoteTrends { get; init; } = [];
    public IReadOnlyList<NicolasLevelStatsDto> NicolasLevelDistribution { get; init; } = [];
}

/// <summary>
/// DTO pour les statistiques par catégorie (record for immutability)
/// </summary>
public record CategoryStatsDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public string CategoryColor { get; init; } = "";
    public string CategoryIcon { get; init; } = "";
    public int ProposalCount { get; init; }
    public int VoteCount { get; init; }
}

/// <summary>
/// DTO pour les statistiques de votes quotidiens (record for immutability)
/// </summary>
public record DailyVoteStatsDto
{
    public DateTime Date { get; init; }
    public int VotesFor { get; init; }
    public int VotesAgainst { get; init; }
    public int TotalVotes => VotesFor + VotesAgainst;
}

/// <summary>
/// DTO pour la distribution des niveaux Nicolas (record for immutability)
/// </summary>
public record NicolasLevelStatsDto
{
    public ContributionLevel Level { get; init; }
    public int Count { get; init; }
    public double Percentage { get; init; }
}

/// <summary>
/// DTO pour les propositions tendances (record for immutability)
/// </summary>
public record TrendingProposalDto
{
    public ProposalDto Proposal { get; init; } = new();
    public int RecentVotes { get; init; }
    public int RecentComments { get; init; }
    public int TrendScore { get; init; }
}

/// <summary>
/// DTO pour les top contributeurs (record for immutability)
/// </summary>
public record TopContributorDto
{
    public string UserId { get; init; } = "";
    public string UserDisplayName { get; init; } = "";
    public ContributionLevel UserContributionLevel { get; init; }
    public int ProposalsCount { get; init; }
    public int VotesCount { get; init; }
    public int CommentsCount { get; init; }
    public int TotalScore { get; init; }
}

/// <summary>
/// DTO pour les tendances de votes (record for immutability)
/// </summary>
public record VotingTrendsDto
{
    public IReadOnlyList<DailyVoteCount> DailyVotes { get; init; } = [];
    public IReadOnlyList<CategoryVoteCount> VotesByCategory { get; init; } = [];
    public IReadOnlyList<ProposalDto> TrendingProposals { get; init; } = [];
}

/// <summary>
/// DTO pour le nombre de votes par jour (record for immutability)
/// </summary>
public record DailyVoteCount
{
    public DateTime Date { get; init; }
    public int VotesFor { get; init; }
    public int VotesAgainst { get; init; }
    public int TotalVotes => VotesFor + VotesAgainst;
}

/// <summary>
/// DTO pour le nombre de votes par catégorie (record for immutability)
/// </summary>
public record CategoryVoteCount
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public string CategoryColor { get; init; } = "";
    public int VoteCount { get; init; }
    public int ProposalCount { get; init; }
}

/// <summary>
/// DTO pour la répartition des niveaux de contribution (record for immutability)
/// </summary>
public record ContributionLevelDistributionDto
{
    public IReadOnlyList<ContributionLevelCount> Distribution { get; init; } = [];
    public double AverageLevel { get; init; }
}

/// <summary>
/// DTO pour le nombre d'utilisateurs par niveau de contribution (record for immutability)
/// </summary>
public record ContributionLevelCount
{
    public string LevelName { get; init; } = "";
    public int UserCount { get; init; }
    public double Percentage { get; init; }
}

/// <summary>
/// DTO pour les top contributeurs (record for immutability)
/// </summary>
public record TopContributorsDto
{
    public IReadOnlyList<UserContributionDto> TopProposers { get; init; } = [];
    public IReadOnlyList<UserContributionDto> TopVoters { get; init; } = [];
    public IReadOnlyList<UserContributionDto> TopCommenters { get; init; } = [];
}

/// <summary>
/// DTO pour la contribution d'un utilisateur (record for immutability)
/// </summary>
public record UserContributionDto
{
    public string UserId { get; init; } = "";
    public string UserDisplayName { get; init; } = "";
    public ContributionLevel UserContributionLevel { get; init; }
    public int ContributionCount { get; init; }
    public int ReputationScore { get; init; }
}

/// <summary>
/// DTO pour l'activité récente (record for immutability)
/// </summary>
public record RecentActivityDto
{
    public IReadOnlyList<RecentActivityItem> Activities { get; init; } = [];
}

/// <summary>
/// DTO pour un élément d'activité récente (record for immutability)
/// </summary>
public record RecentActivityItem
{
    public string Type { get; init; } = ""; // "Proposal", "Vote", "Comment"
    public string Description { get; init; } = "";
    public string UserId { get; init; } = "";
    public string UserDisplayName { get; init; } = "";
    public DateTime Timestamp { get; init; }
    public string? RelatedItemId { get; init; }
    public string? RelatedItemTitle { get; init; }
}

/// <summary>
/// DTO pour le baromètre de frustration (record for immutability)
/// </summary>
public record FrustrationBarometerDto
{
    public double FrustrationLevel { get; init; } // Percentage 0-100
    public int TotalVotesAgainst { get; init; }
    public int TotalVotes { get; init; }
    public IReadOnlyList<CategoryFrustration> CategoryBreakdown { get; init; } = [];
    public string CurrentMood { get; init; } = ""; // "Calm", "Frustrated", "Angry", etc.
}

/// <summary>
/// DTO pour la frustration par catégorie (record for immutability)
/// </summary>
public record CategoryFrustration
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public double FrustrationLevel { get; init; }
    public int VotesAgainst { get; init; }
    public int TotalVotes { get; init; }
}