namespace NicolasQuiPaieData.DTOs;

// DTOs should include their own enum definitions to avoid dependencies
public enum ContributionLevel
{
    PetitNicolas = 1,
    GrosMoyenNicolas = 2,
    GrosNicolas = 3,
    NicolasSupreme = 4
}

public enum ProposalStatus
{
    Draft,
    Active,
    Closed,
    Archived
}

public enum VoteType
{
    Against = 0,
    For = 1
}

public enum LogLevel
{
    Verbose = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Fatal = 5
}

/// <summary>
/// DTO pour les logs API (record for immutability during transfer)
/// </summary>
public record ApiLogDto
{
    public int Id { get; init; }
    public string? Message { get; init; }
    public string Level { get; init; } = "";
    public DateTime TimeStamp { get; init; }
    public string? Exception { get; init; }
    public string? UserId { get; init; }
    public string? UserName { get; init; }
    public string? RequestPath { get; init; }
    public string? RequestMethod { get; init; }
    public int? StatusCode { get; init; }
    public long? Duration { get; init; }
    public string? ClientIP { get; init; }
    public string? Source { get; init; }
}

/// <summary>
/// DTO pour la réponse des logs API (record for immutability)
/// </summary>
public record ApiLogsResponseDto
{
    public int TotalReturned { get; init; }
    public string? LevelFilter { get; init; }
    public int RequestedCount { get; init; }
    public string[] AvailableLevels { get; init; } = [];
    public ApiLogDto[] Logs { get; init; } = [];
}

/// <summary>
/// DTO pour les propositions (record for immutability during transfer)
/// </summary>
public record ProposalDto
{
    public int Id { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public ProposalStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public int VotesFor { get; init; }
    public int VotesAgainst { get; init; }
    public int ViewsCount { get; init; }
    public bool IsFeatured { get; init; }
    public string? ImageUrl { get; init; }
    public string? Tags { get; init; }

    // Propriétés liées au créateur
    public string CreatedById { get; init; } = "";
    public string CreatedByDisplayName { get; init; } = "";

    // Propriétés liées à la catégorie
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public string CategoryColor { get; init; } = "";
    public string CategoryIcon { get; init; } = "";

    // Propriétés calculées
    public int TotalVotes => VotesFor + VotesAgainst;
    public double ApprovalRate => TotalVotes > 0 ? (double)VotesFor / TotalVotes * 100 : 0;
    public bool IsHot => TotalVotes > 50 && CreatedAt > DateTime.UtcNow.AddDays(-3);
}

/// <summary>
/// DTO pour créer une proposition (class with setters for form binding)
/// </summary>
public class CreateProposalDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO pour mettre à jour une proposition (class with setters for form binding)
/// </summary>
public class UpdateProposalDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO pour les votes (record for immutability during transfer)
/// </summary>
public record VoteDto
{
    public int Id { get; init; }
    public VoteType VoteType { get; init; }
    public DateTime VotedAt { get; init; }
    public int Weight { get; init; }
    public string? Comment { get; init; }
    public string UserId { get; init; } = "";
    public int ProposalId { get; init; }
    public ProposalDto? Proposal { get; init; }
}

/// <summary>
/// DTO pour créer un vote (class with setters for form binding)
/// </summary>
public class CreateVoteDto
{
    public VoteType VoteType { get; set; }
    public int ProposalId { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// DTO pour les commentaires (record for immutability during transfer)
/// </summary>
public record CommentDto
{
    public int Id { get; init; }
    public string Content { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }
    public bool IsModerated { get; init; }
    public int LikesCount { get; init; }
    public string UserId { get; init; } = "";
    public string UserDisplayName { get; init; } = "";
    public int ProposalId { get; init; }
    public int? ParentCommentId { get; init; }
    public IReadOnlyCollection<CommentDto> Replies { get; init; } = [];
}

/// <summary>
/// DTO pour créer un commentaire (class with setters for form binding)
/// </summary>
public class CreateCommentDto
{
    public string Content { get; set; } = "";
    public int ProposalId { get; set; }
    public int? ParentCommentId { get; set; }
}

/// <summary>
/// DTO pour mettre à jour un commentaire (class with setters for form binding)
/// </summary>
public class UpdateCommentDto
{
    public string Content { get; set; } = "";
}

/// <summary>
/// DTO pour les catégories (record for immutability during transfer)
/// </summary>
public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Color { get; init; } = "";
    public string IconClass { get; init; } = "";
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public int ProposalsCount { get; init; }
}

/// <summary>
/// DTO pour les utilisateurs (record for immutability during transfer)
/// </summary>
public record UserDto
{
    public string Id { get; init; } = "";
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? Bio { get; init; }
    public ContributionLevel ContributionLevel { get; init; }
    public int ReputationScore { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public bool IsVerified { get; init; }
    public string? ProfileImageUrl { get; init; }
}

/// <summary>
/// DTO pour mettre à jour le profil utilisateur (class with setters for form binding)
/// </summary>
public class UpdateUserProfileDto
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
}

/// <summary>
/// DTO pour les statistiques utilisateur (record for immutability)
/// </summary>
public record UserStatsDto
{
    public int ProposalsCount { get; init; }
    public int VotesCount { get; init; }
    public int CommentsCount { get; init; }
    public int ReputationScore { get; init; }
}

/// <summary>
/// DTO pour les accomplissements (record for immutability)
/// </summary>
public record AchievementDto
{
    public string Icon { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public DateTime? UnlockedAt { get; init; }
}

/// <summary>
/// DTO pour un élément d'activité (record for immutability)
/// </summary>
public record ActivityItemDto
{
    public string Type { get; init; } = "";
    public string Title { get; init; } = "";
    public string UserName { get; init; } = "";
    public DateTime CreatedAt { get; init; }
}