using System.ComponentModel.DataAnnotations.Schema;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class CommentLike
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        
        public int CommentId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        
        [ForeignKey(nameof(CommentId))]
        public virtual Comment Comment { get; set; } = null!;
    }
}