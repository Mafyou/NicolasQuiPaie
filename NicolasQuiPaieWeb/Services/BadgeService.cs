using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieWeb.Services
{
    /// <summary>
    /// Client-side placeholder service for badges - will be replaced with API calls when available
    /// </summary>
    public class BadgeService
    {
        private readonly ILogger<BadgeService> _logger;

        public BadgeService(ILogger<BadgeService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> EvaluateAndUpdateUserBadgeAsync(string userId)
        {
            // TODO: Replace with API call when badge evaluation endpoint is available
            await Task.Delay(100); // Simulate API call
            
            _logger.LogInformation("Badge evaluation for user {UserId} - placeholder implementation", userId);
            return false; // No change for now
        }

        public string GetBadgeDisplayName(FiscalLevel level)
        {
            return level switch
            {
                FiscalLevel.PetitNicolas => "?? Petit Nicolas",
                FiscalLevel.GrosMoyenNicolas => "?? Moyen Nicolas", 
                FiscalLevel.GrosNicolas => "?? Gros Nicolas",
                FiscalLevel.NicolasSupreme => "?? Nicolas Suprême",
                _ => "?? Petit Nicolas"
            };
        }

        public string GetBadgeDescription(FiscalLevel level)
        {
            return level switch
            {
                FiscalLevel.PetitNicolas => "Badge de débutant - Reconnaissance pour vos premiers pas dans la communauté Nicolas",
                FiscalLevel.GrosMoyenNicolas => "Badge de contributeur moyen - Participation régulière reconnue",
                FiscalLevel.GrosNicolas => "Badge de contributeur actif - Reconnaissance de votre participation régulière et engagement",
                FiscalLevel.NicolasSupreme => "Badge d'expert - Reconnaissance de votre expertise et contribution exceptionnelle à la communauté",
                _ => "Badge de contributeur Nicolas"
            };
        }
    }
}