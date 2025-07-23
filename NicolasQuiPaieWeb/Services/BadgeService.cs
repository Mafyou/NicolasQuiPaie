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
        /// �value et met � jour automatiquement le badge de contribution d'un utilisateur
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
                    
                    return true; // Badge chang�
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
        /// Calcule le niveau de badge de contribution selon l'activit� de l'utilisateur
        /// </summary>
        private FiscalLevel CalculateContributionLevel(ApplicationUser user)
        {
            var score = CalculateContributionScore(user);

            // Crit�res d'�volution des badges de contribution
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
                return FiscalLevel.PetitNicolas; // ?? Contributeur D�butant
            }
        }

        /// <summary>
        /// Calcule le score de contribution d'un utilisateur
        /// </summary>
        private int CalculateContributionScore(ApplicationUser user)
        {
            var score = 0;

            // Points pour les propositions cr��es (encourager la cr�ation de contenu)
            score += user.CreatedProposals.Count * 100;

            // Points pour les votes (participation d�mocratique)
            var recentVotes = user.Votes.Count(v => v.VotedAt >= DateTime.UtcNow.AddDays(-30));
            score += recentVotes * 10;
            score += user.Votes.Count * 5;

            // Points pour les commentaires constructifs
            var recentComments = user.Comments.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30) && !c.IsDeleted);
            score += recentComments * 15;
            score += user.Comments.Count(c => !c.IsDeleted) * 8;

            // Bonus de r�putation (qualit� reconnue par la communaut�)
            score += user.ReputationScore;

            // Bonus d'anciennet� (fid�lit� � la plateforme)
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
                FiscalLevel.NicolasSupreme => "?? Nicolas Supr�me",
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
                FiscalLevel.PetitNicolas => "Badge de d�butant - Reconnaissance pour vos premiers pas dans la communaut� Nicolas",
                FiscalLevel.GrosNicolas => "Badge de contributeur actif - Reconnaissance de votre participation r�guli�re et engagement",
                FiscalLevel.NicolasSupreme => "Badge d'expert - Reconnaissance de votre expertise et contribution exceptionnelle � la communaut�",
                _ => "Badge de contributeur Nicolas"
            };
        }

        /// <summary>
        /// Obtient les crit�res pour le niveau sup�rieur
        /// </summary>
        public string GetNextLevelCriteria(FiscalLevel currentLevel)
        {
            return currentLevel switch
            {
                FiscalLevel.PetitNicolas => "Pour devenir Gros Nicolas : 300 points de contribution, 100 r�putation, 2 propositions cr��es",
                FiscalLevel.GrosNicolas => "Pour devenir Nicolas Supr�me : 1000 points de contribution, 500 r�putation, 5 propositions cr��es",
                FiscalLevel.NicolasSupreme => "Vous avez atteint le niveau maximum de reconnaissance ! ?? Merci pour votre contribution exceptionnelle !",
                _ => "Continuez � participer pour faire �voluer votre badge de contribution"
            };
        }

        /// <summary>
        /// Message de clarification sur le r�le des badges
        /// </summary>
        public string GetBadgeExplanation()
        {
            return "?? Important : Les badges reconnaissent votre niveau de contribution mais n'affectent PAS le poids de vos votes. " +
                   "Chaque Nicolas a une voix �gale : 1 vote = 1 voix, peu importe le badge !";
        }

        /// <summary>
        /// �value tous les utilisateurs (t�che p�riodique)
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