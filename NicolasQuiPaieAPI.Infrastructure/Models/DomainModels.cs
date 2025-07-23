using Microsoft.AspNetCore.Identity;

namespace NicolasQuiPaieAPI.Infrastructure.Models
{
    public enum FiscalLevel
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

    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public FiscalLevel FiscalLevel { get; set; } = FiscalLevel.PetitNicolas;
        public int ReputationScore { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public bool IsVerified { get; set; } = false;
        public string? ProfileImageUrl { get; set; }

        // Navigation properties
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    }

    public class Proposal
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public ProposalStatus Status { get; set; } = ProposalStatus.Draft;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int VotesFor { get; set; } = 0;
        public int VotesAgainst { get; set; } = 0;
        public int ViewsCount { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
        public string? ImageUrl { get; set; }
        public string? Tags { get; set; }

        // Foreign keys
        public string CreatedById { get; set; } = "";
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Computed properties for convenience
        public int TotalVotes => VotesFor + VotesAgainst;
        public double ApprovalRate => TotalVotes > 0 ? (double)VotesFor / TotalVotes * 100 : 0;
        public bool IsHot => TotalVotes > 50 && CreatedAt > DateTime.UtcNow.AddDays(-3);
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Color { get; set; } = "#007bff";
        public string IconClass { get; set; } = "fas fa-folder";
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }

    public class Vote
    {
        public int Id { get; set; }
        public VoteType VoteType { get; set; }
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        public int Weight { get; set; } = 1;
        public string? Comment { get; set; }

        // Foreign keys
        public string UserId { get; set; } = "";
        public int ProposalId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Proposal Proposal { get; set; } = null!;
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsModerated { get; set; } = false;
        public int LikesCount { get; set; } = 0;

        // Foreign keys
        public string UserId { get; set; } = "";
        public int ProposalId { get; set; }
        public int? ParentCommentId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Proposal Proposal { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }

    public class CommentLike
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public string UserId { get; set; } = "";
        public int CommentId { get; set; }

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Comment Comment { get; set; } = null!;
    }
}