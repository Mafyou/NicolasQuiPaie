using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class Vote
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int ProposalId { get; set; }
        
        public VoteType VoteType { get; set; }
        
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
        
        public int Weight { get; set; } = 1;
        
        public string? Comment { get; set; }
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey(nameof(ProposalId))]
        public virtual Proposal Proposal { get; set; } = null!;
    }

    public enum VoteType
    {
        Against = 0,
        For = 1,
        Abstain = 2
    }
}