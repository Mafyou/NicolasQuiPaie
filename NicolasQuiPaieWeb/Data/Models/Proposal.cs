using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class Proposal
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        
        [Required]
        public string CreatedById { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ProposalStatus Status { get; set; } = ProposalStatus.Active;
        
        public int VotesFor { get; set; } = 0;
        
        public int VotesAgainst { get; set; } = 0;
        
        public int ViewsCount { get; set; } = 0;
        
        public bool IsFeatured { get; set; } = false;
        
        public DateTime? ClosedAt { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(CreatedById))]
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;
        
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        
        // Computed properties
        public int TotalVotes => VotesFor + VotesAgainst;
        public double ApprovalRate => TotalVotes > 0 ? (double)VotesFor / TotalVotes * 100 : 0;
        public bool IsHot => CreatedAt > DateTime.UtcNow.AddDays(-1) && TotalVotes > 10;
    }

    public enum ProposalStatus
    {
        Draft = 0,
        Active = 1,
        Closed = 2,
        Archived = 3
    }
}