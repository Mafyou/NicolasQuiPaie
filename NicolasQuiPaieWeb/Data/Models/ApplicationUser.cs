using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string? DisplayName { get; set; }
        
        public FiscalLevel FiscalLevel { get; set; } = FiscalLevel.PetitNicolas;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsVerified { get; set; } = false;
        
        public int ReputationScore { get; set; } = 0;
        
        [StringLength(500)]
        public string? Bio { get; set; }
        
        public string? AvatarUrl { get; set; }
        
        // Navigation properties
        public virtual ICollection<Proposal> CreatedProposals { get; set; } = new List<Proposal>();
        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    }

    public enum FiscalLevel
    {
        PetitNicolas = 1,
        GrosNicolas = 2,
        NicolasSupreme = 3
    }
}