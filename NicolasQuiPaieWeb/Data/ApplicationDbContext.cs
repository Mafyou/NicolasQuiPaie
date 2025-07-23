using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieWeb.Data.Models;

namespace NicolasQuiPaieWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure unique constraints
            builder.Entity<Vote>()
                .HasIndex(v => new { v.UserId, v.ProposalId })
                .IsUnique();

            builder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.UserId, cl.CommentId })
                .IsUnique();

            // Configure relationships with cascade behavior to avoid cycles
            
            // Comment -> ParentComment: Restrict to avoid self-referencing cascade
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Vote: Cascade (direct relationship)
            builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .OnDelete(DeleteBehavior.Cascade);

            // Proposal -> Vote: Cascade
            builder.Entity<Vote>()
                .HasOne(v => v.Proposal)
                .WithMany(p => p.Votes)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Comment: NoAction to avoid cascade cycle
            // (Comments will be handled via Proposal cascade or manual cleanup)
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .OnDelete(DeleteBehavior.NoAction);

            // Proposal -> Comment: Cascade
            builder.Entity<Comment>()
                .HasOne(c => c.Proposal)
                .WithMany(p => p.Comments)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> CommentLike: NoAction to avoid cascade cycle
            builder.Entity<CommentLike>()
                .HasOne(cl => cl.User)
                .WithMany(u => u.CommentLikes)
                .OnDelete(DeleteBehavior.NoAction);

            // Comment -> CommentLike: Cascade
            builder.Entity<CommentLike>()
                .HasOne(cl => cl.Comment)
                .WithMany(c => c.Likes)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Proposal: NoAction to preserve data integrity
            // (Proposals should remain even if creator is deleted, with anonymization)
            builder.Entity<Proposal>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProposals)
                .OnDelete(DeleteBehavior.NoAction);

            // Category -> Proposal: Restrict to prevent accidental category deletion
            builder.Entity<Proposal>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Proposals)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fiscalité", Description = "Impôts, taxes et prélèvements", IconClass = "fas fa-coins", Color = "#ff6b6b", SortOrder = 1 },
                new Category { Id = 2, Name = "Dépenses Publiques", Description = "Budget de l'État et collectivités", IconClass = "fas fa-university", Color = "#4ecdc4", SortOrder = 2 },
                new Category { Id = 3, Name = "Santé", Description = "Système de santé et sécurité sociale", IconClass = "fas fa-heartbeat", Color = "#45b7d1", SortOrder = 3 },
                new Category { Id = 4, Name = "Éducation", Description = "École, université et formation", IconClass = "fas fa-graduation-cap", Color = "#f9ca24", SortOrder = 4 },
                new Category { Id = 5, Name = "Transport", Description = "Infrastructure et transport public", IconClass = "fas fa-subway", Color = "#6c5ce7", SortOrder = 5 },
                new Category { Id = 6, Name = "Environnement", Description = "Écologie et transition énergétique", IconClass = "fas fa-leaf", Color = "#00b894", SortOrder = 6 },
                new Category { Id = 7, Name = "Sécurité", Description = "Police, justice et défense", IconClass = "fas fa-shield-alt", Color = "#e17055", SortOrder = 7 },
                new Category { Id = 8, Name = "Logement", Description = "Politique du logement", IconClass = "fas fa-home", Color = "#fdcb6e", SortOrder = 8 },
                new Category { Id = 9, Name = "Culture", Description = "Arts, médias et patrimoine", IconClass = "fas fa-palette", Color = "#e84393", SortOrder = 9 },
                new Category { Id = 10, Name = "Autres", Description = "Autres sujets de société", IconClass = "fas fa-question-circle", Color = "#636e72", SortOrder = 10 }
            );
        }
    }
}