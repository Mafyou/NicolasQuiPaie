using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int ProposalId { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int? ParentCommentId { get; set; }
        
        public int LikesCount { get; set; } = 0;
        
        public bool IsDeleted { get; set; } = false;
        
        public bool IsModerated { get; set; } = false;
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey(nameof(ProposalId))]
        public virtual Proposal Proposal { get; set; } = null!;
        
        [ForeignKey(nameof(ParentCommentId))]
        public virtual Comment? ParentComment { get; set; }
        
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}