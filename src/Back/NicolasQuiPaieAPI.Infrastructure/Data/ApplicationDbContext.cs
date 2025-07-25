using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieAPI.Infrastructure.Models;

namespace NicolasQuiPaieAPI.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Proposal> Proposals { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Vote> Votes { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<CommentLike> CommentLikes { get; set; } = null!;
        public DbSet<ApiLog> ApiLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration des entités
            ConfigureProposal(builder);
            ConfigureVote(builder);
            ConfigureComment(builder);
            ConfigureCommentLike(builder);
            ConfigureCategory(builder);
            ConfigureApiLog(builder);

            // Données de test
            SeedData(builder);
        }

        private static void ConfigureProposal(ModelBuilder builder)
        {
            builder.Entity<Proposal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.Status).IsRequired();

                entity.HasOne(p => p.CreatedBy)
                    .WithMany(u => u.Proposals)
                    .HasForeignKey(p => p.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Proposals)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CategoryId);
            });
        }

        private static void ConfigureVote(ModelBuilder builder)
        {
            builder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(v => v.User)
                    .WithMany(u => u.Votes)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Proposal)
                    .WithMany(p => p.Votes)
                    .HasForeignKey(v => v.ProposalId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Un utilisateur ne peut voter qu'une fois par proposition
                entity.HasIndex(e => new { e.UserId, e.ProposalId }).IsUnique();
                entity.HasIndex(e => e.VotedAt);
            });
        }

        private static void ConfigureComment(ModelBuilder builder)
        {
            builder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Proposal)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.ProposalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ProposalId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }

        private static void ConfigureCommentLike(ModelBuilder builder)
        {
            builder.Entity<CommentLike>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(cl => cl.User)
                    .WithMany(u => u.CommentLikes)
                    .HasForeignKey(cl => cl.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cl => cl.Comment)
                    .WithMany(c => c.Likes)
                    .HasForeignKey(cl => cl.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Un utilisateur ne peut liker qu'une fois par commentaire
                entity.HasIndex(e => new { e.UserId, e.CommentId }).IsUnique();
            });
        }

        private static void ConfigureCategory(ModelBuilder builder)
        {
            builder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(7);
                entity.Property(e => e.IconClass).HasMaxLength(50);

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.SortOrder);
            });
        }

        private static void ConfigureApiLog(ModelBuilder builder)
        {
            builder.Entity<ApiLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).HasMaxLength(4000);
                entity.Property(e => e.MessageTemplate).HasMaxLength(2000);
                entity.Property(e => e.Level).IsRequired();
                entity.Property(e => e.TimeStamp).IsRequired();
                entity.Property(e => e.Exception).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
                
                // Additional properties constraints
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.UserName).HasMaxLength(256);
                entity.Property(e => e.RequestPath).HasMaxLength(2048);
                entity.Property(e => e.RequestMethod).HasMaxLength(10);
                entity.Property(e => e.ClientIP).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(1000);
                entity.Property(e => e.Source).HasMaxLength(100);

                // Indexes for efficient querying
                entity.HasIndex(e => e.TimeStamp);
                entity.HasIndex(e => e.Level);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.Level, e.TimeStamp });
            });
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Catégories par défaut
            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Fiscalité",
                    Description = "Propositions liées aux impôts et taxes",
                    Color = "#dc3545",
                    IconClass = "fas fa-coins",
                    SortOrder = 1
                },
                new Category
                {
                    Id = 2,
                    Name = "Dépenses Publiques",
                    Description = "Propositions sur l'utilisation de l'argent public",
                    Color = "#28a745",
                    IconClass = "fas fa-hand-holding-usd",
                    SortOrder = 2
                },
                new Category
                {
                    Id = 3,
                    Name = "Services Publics",
                    Description = "Amélioration des services publics",
                    Color = "#007bff",
                    IconClass = "fas fa-building",
                    SortOrder = 3
                },
                new Category
                {
                    Id = 4,
                    Name = "Infrastructure",
                    Description = "Investissements dans les infrastructures",
                    Color = "#fd7e14",
                    IconClass = "fas fa-road",
                    SortOrder = 4
                },
                new Category
                {
                    Id = 5,
                    Name = "Éducation",
                    Description = "Financement et réforme de l'éducation",
                    Color = "#6f42c1",
                    IconClass = "fas fa-graduation-cap",
                    SortOrder = 5
                },
                new Category
                {
                    Id = 6,
                    Name = "Santé",
                    Description = "Système de santé et financement",
                    Color = "#20c997",
                    IconClass = "fas fa-heartbeat",
                    SortOrder = 6
                }
            );
        }
    }
}