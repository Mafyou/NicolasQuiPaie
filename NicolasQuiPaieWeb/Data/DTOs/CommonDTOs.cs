using NicolasQuiPaieWeb.Data.Models;

namespace NicolasQuiPaieWeb.Data.DTOs
{
    /// <summary>
    /// DTO pour les statistiques utilisateur - évite les problèmes de lazy loading
    /// </summary>
    public class UserStatsDto
    {
        public int ProposalsCount { get; set; }
        public int VotesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    /// <summary>
    /// DTO pour les accomplissements utilisateur
    /// </summary>
    public class AchievementDto
    {
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// DTO pour les votes - évite les problèmes de concurrence DbContext
    /// </summary>
    public class VoteDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public int ProposalId { get; set; }
        public VoteType VoteType { get; set; }
        public DateTime VotedAt { get; set; }
        public int Weight { get; set; }
        public string? Comment { get; set; }
        public ProposalDto? Proposal { get; set; }
    }

    /// <summary>
    /// DTO pour les propositions - évite les problèmes de concurrence DbContext
    /// </summary>
    public class ProposalDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public ProposalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VotesFor { get; set; }
        public int VotesAgainst { get; set; }
        public int ViewsCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        // Propriétés calculées safe
        public int TotalVotes => VotesFor + VotesAgainst;
        public double ApprovalRate => TotalVotes > 0 ? (double)VotesFor / TotalVotes * 100 : 0;
        public bool IsHot => CreatedAt > DateTime.UtcNow.AddDays(-1) && TotalVotes > 10;
        
        // Navigation properties as DTOs
        public string? CreatedById { get; set; }
        public string? CreatedByDisplayName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public string? CategoryIcon { get; set; }
    }

    /// <summary>
    /// DTO pour les commentaires - évite les problèmes de concurrence DbContext
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string UserDisplayName { get; set; } = "";
        public int ProposalId { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public int LikesCount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModerated { get; set; }
        
        // Collections pour les réponses
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}