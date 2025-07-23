using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieWeb.Data;
using NicolasQuiPaieWeb.Data.Models;

namespace NicolasQuiPaieWeb.Services
{
    public class BadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BadgeService> _logger;

        public BadgeService(ApplicationDbContext context, ILogger<BadgeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Évalue et met à jour automatiquement le badge de contribution d'un utilisateur
        /// IMPORTANT: Les badges ne changent PAS le poids des votes, ils reconnaissent la contribution
        /// </summary>
        public async Task<bool> EvaluateAndUpdateUserBadgeAsync(string userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.CreatedProposals)
                    .Include(u => u.Votes)
                    .Include(u => u.Comments)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return false;

                var currentBadge = user.FiscalLevel;
                var newBadge = CalculateContributionLevel(user);

                if (newBadge != currentBadge)
                {
                    user.FiscalLevel = newBadge;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("User {UserId} contribution badge updated from {OldBadge} to {NewBadge}", 
                        userId, currentBadge, newBadge);
                    
                    return true; // Badge changé
                }

                return false; // Pas de changement
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating contribution badge for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Calcule le niveau de badge de contribution selon l'activité de l'utilisateur
        /// </summary>
        private FiscalLevel CalculateContributionLevel(ApplicationUser user)
        {
            var score = CalculateContributionScore(user);

            // Critères d'évolution des badges de contribution
            if (score >= 1000 && user.ReputationScore >= 500 && user.CreatedProposals.Count >= 5)
            {
                return FiscalLevel.NicolasSupreme; // ?? Contributeur Expert
            }
            else if (score >= 300 && user.ReputationScore >= 100 && user.CreatedProposals.Count >= 2)
            {
                return FiscalLevel.GrosNicolas; // ?? Contributeur Actif
            }
            else
            {
                return FiscalLevel.PetitNicolas; // ?? Contributeur Débutant
            }
        }

        /// <summary>
        /// Calcule le score de contribution d'un utilisateur
        /// </summary>
        private int CalculateContributionScore(ApplicationUser user)
        {
            var score = 0;

            // Points pour les propositions créées (encourager la création de contenu)
            score += user.CreatedProposals.Count * 100;

            // Points pour les votes (participation démocratique)
            var recentVotes = user.Votes.Count(v => v.VotedAt >= DateTime.UtcNow.AddDays(-30));
            score += recentVotes * 10;
            score += user.Votes.Count * 5;

            // Points pour les commentaires constructifs
            var recentComments = user.Comments.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30) && !c.IsDeleted);
            score += recentComments * 15;
            score += user.Comments.Count(c => !c.IsDeleted) * 8;

            // Bonus de réputation (qualité reconnue par la communauté)
            score += user.ReputationScore;

            // Bonus d'ancienneté (fidélité à la plateforme)
            var daysSinceJoin = (DateTime.UtcNow - user.CreatedAt).Days;
            if (daysSinceJoin >= 30) score += 50;   // 1 mois
            if (daysSinceJoin >= 90) score += 100;  // 3 mois
            if (daysSinceJoin >= 365) score += 200; // 1 an

            return score;
        }

        /// <summary>
        /// Obtient le nom du badge de contribution avec emoji
        /// </summary>
        public string GetBadgeDisplayName(FiscalLevel level)
        {
            return level switch
            {
                FiscalLevel.PetitNicolas => "?? Petit Nicolas",
                FiscalLevel.GrosNicolas => "?? Gros Nicolas", 
                FiscalLevel.NicolasSupreme => "?? Nicolas Suprême",
                _ => "?? Petit Nicolas"
            };
        }

        /// <summary>
        /// Obtient la description du badge de contribution
        /// </summary>
        public string GetBadgeDescription(FiscalLevel level)
        {
            return level switch
            {
                FiscalLevel.PetitNicolas => "Badge de débutant - Reconnaissance pour vos premiers pas dans la communauté Nicolas",
                FiscalLevel.GrosNicolas => "Badge de contributeur actif - Reconnaissance de votre participation régulière et engagement",
                FiscalLevel.NicolasSupreme => "Badge d'expert - Reconnaissance de votre expertise et contribution exceptionnelle à la communauté",
                _ => "Badge de contributeur Nicolas"
            };
        }

        /// <summary>
        /// Obtient les critères pour le niveau supérieur
        /// </summary>
        public string GetNextLevelCriteria(FiscalLevel currentLevel)
        {
            return currentLevel switch
            {
                FiscalLevel.PetitNicolas => "Pour devenir Gros Nicolas : 300 points de contribution, 100 réputation, 2 propositions créées",
                FiscalLevel.GrosNicolas => "Pour devenir Nicolas Suprême : 1000 points de contribution, 500 réputation, 5 propositions créées",
                FiscalLevel.NicolasSupreme => "Vous avez atteint le niveau maximum de reconnaissance ! ?? Merci pour votre contribution exceptionnelle !",
                _ => "Continuez à participer pour faire évoluer votre badge de contribution"
            };
        }

        /// <summary>
        /// Message de clarification sur le rôle des badges
        /// </summary>
        public string GetBadgeExplanation()
        {
            return "?? Important : Les badges reconnaissent votre niveau de contribution mais n'affectent PAS le poids de vos votes. " +
                   "Chaque Nicolas a une voix égale : 1 vote = 1 voix, peu importe le badge !";
        }

        /// <summary>
        /// Évalue tous les utilisateurs (tâche périodique)
        /// </summary>
        public async Task<int> EvaluateAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            var updatedCount = 0;

            foreach (var user in users)
            {
                var wasUpdated = await EvaluateAndUpdateUserBadgeAsync(user.Id);
                if (wasUpdated) updatedCount++;
            }

            _logger.LogInformation("Contribution badge evaluation completed. {UpdatedCount} users updated out of {TotalUsers}", 
                updatedCount, users.Count);

            return updatedCount;
        }
    }
}