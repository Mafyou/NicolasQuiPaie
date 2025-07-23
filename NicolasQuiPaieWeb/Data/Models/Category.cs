using System.ComponentModel.DataAnnotations;

namespace NicolasQuiPaieWeb.Data.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string IconClass { get; set; } = "fas fa-folder";
        
        [StringLength(20)]
        public string Color { get; set; } = "#007bff";
        
        public int SortOrder { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();
    }
}