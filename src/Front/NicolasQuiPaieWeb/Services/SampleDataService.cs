using NicolasQuiPaieData.DTOs;
using Microsoft.Extensions.Logging;

namespace NicolasQuiPaieWeb.Services
{
    public class SampleDataService
    {
        private readonly List<ProposalDto> _sampleProposals;
        private readonly List<CategoryDto> _sampleCategories;
        private readonly DashboardStatsDto _sampleStats;

        public SampleDataService()
        {
            _sampleCategories = CreateSampleCategories();
            _sampleProposals = CreateSampleProposals();
            _sampleStats = CreateSampleStats();
        }

        public Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
        {
            var filtered = _sampleProposals.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
            {
                filtered = filtered.Where(p => p.CategoryName.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(p => 
                    p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var result = filtered.Skip(skip).Take(take).ToList();
            return Task.FromResult<IEnumerable<ProposalDto>>(result);
        }

        public Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
        {
            var trending = _sampleProposals
                .OrderByDescending(p => p.TotalVotes)
                .Take(take)
                .ToList();
            return Task.FromResult<IEnumerable<ProposalDto>>(trending);
        }

        public Task<ProposalDto?> GetProposalDtoByIdAsync(int id)
        {
            var proposal = _sampleProposals.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(proposal);
        }

        public Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return Task.FromResult<IEnumerable<CategoryDto>>(_sampleCategories);
        }

        public Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            return Task.FromResult(_sampleStats);
        }

        /// <summary>
        /// M�thode de test pour v�rifier que les donn�es sont g�n�r�es correctement
        /// </summary>
        public void LogSampleData(ILogger logger)
        {
            logger.LogInformation("=== SAMPLE DATA DEBUG ===");
            logger.LogInformation("TopCategories Count: {Count}", _sampleStats.TopCategories.Count);
            foreach (var category in _sampleStats.TopCategories)
            {
                logger.LogInformation("Category: {Name}, VoteCount: {VoteCount}, Color: {Color}", 
                    category.CategoryName, category.VoteCount, category.CategoryColor);
            }
            logger.LogInformation("NicolasLevelDistribution Count: {Count}", _sampleStats.NicolasLevelDistribution.Count);
            foreach (var level in _sampleStats.NicolasLevelDistribution)
            {
                logger.LogInformation("Level: {Level}, Count: {Count}, Percentage: {Percentage}%", 
                    level.Level, level.Count, level.Percentage);
            }
            logger.LogInformation("=========================");
        }

        private List<CategoryDto> CreateSampleCategories()
        {
            return new List<CategoryDto>
            {
                new() { Id = 1, Name = "�conomie", Description = "Propositions �conomiques et fiscales", Color = "#e74c3c", IconClass = "fas fa-coins" },
                new() { Id = 2, Name = "Environnement", Description = "�cologie et d�veloppement durable", Color = "#27ae60", IconClass = "fas fa-leaf" },
                new() { Id = 3, Name = "Social", Description = "Questions sociales et soci�tales", Color = "#3498db", IconClass = "fas fa-users" },
                new() { Id = 4, Name = "Num�rique", Description = "Transformation num�rique et tech", Color = "#9b59b6", IconClass = "fas fa-laptop" },
                new() { Id = 5, Name = "�ducation", Description = "Syst�me �ducatif et formation", Color = "#f39c12", IconClass = "fas fa-graduation-cap" },
                new() { Id = 6, Name = "Sant�", Description = "Syst�me de sant� publique", Color = "#e67e22", IconClass = "fas fa-heartbeat" }
            };
        }

        private List<ProposalDto> CreateSampleProposals()
        {
            return new List<ProposalDto>
            {
                new()
                {
                    Id = 1,
                    Title = "R�duction de la TVA sur les produits bio",
                    Description = "Proposition pour r�duire la TVA de 20% � 5,5% sur tous les produits alimentaires biologiques certifi�s pour encourager une alimentation saine et durable.",
                    CategoryId = 1,
                    CategoryName = "�conomie",
                    CategoryColor = "#e74c3c",
                    CategoryIcon = "fas fa-coins",
                    VotesFor = 1247,
                    VotesAgainst = 156,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    ViewsCount = 3420,
                    CreatedByDisplayName = "Marie Dubois",
                    CreatedById = "user1",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 2,
                    Title = "Interdiction des jets priv�s pour les trajets courts",
                    Description = "Interdire l'utilisation de jets priv�s pour tous les trajets de moins de 500km en Europe, avec alternatives en train haute vitesse obligatoires.",
                    CategoryId = 2,
                    CategoryName = "Environnement",
                    CategoryColor = "#27ae60",
                    CategoryIcon = "fas fa-leaf",
                    VotesFor = 2134,
                    VotesAgainst = 432,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    ViewsCount = 5678,
                    CreatedByDisplayName = "Pierre �colo",
                    CreatedById = "user2",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 3,
                    Title = "Semaine de 4 jours obligatoire",
                    Description = "Instaurer une semaine de travail de 32 heures (4 jours) sans r�duction de salaire pour am�liorer la qualit� de vie et la productivit�.",
                    CategoryId = 3,
                    CategoryName = "Social",
                    CategoryColor = "#3498db",
                    CategoryIcon = "fas fa-users",
                    VotesFor = 3456,
                    VotesAgainst = 1234,
                    CreatedAt = DateTime.Now.AddDays(-7),
                    ViewsCount = 8901,
                    CreatedByDisplayName = "Jean Travailleur",
                    CreatedById = "user3",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 4,
                    Title = "Internet gratuit dans tous les lieux publics",
                    Description = "D�ployer un r�seau WiFi public gratuit et s�curis� dans toutes les communes de France pour r�duire la fracture num�rique.",
                    CategoryId = 4,
                    CategoryName = "Num�rique",
                    CategoryColor = "#9b59b6",
                    CategoryIcon = "fas fa-laptop",
                    VotesFor = 1876,
                    VotesAgainst = 287,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    ViewsCount = 4567,
                    CreatedByDisplayName = "Sophie Connect�e",
                    CreatedById = "user4",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 5,
                    Title = "Cours d'�ducation financi�re obligatoire",
                    Description = "Rendre obligatoires les cours d'�ducation financi�re d�s le coll�ge pour apprendre la gestion budg�taire, les investissements et les imp�ts.",
                    CategoryId = 5,
                    CategoryName = "�ducation",
                    CategoryColor = "#f39c12",
                    CategoryIcon = "fas fa-graduation-cap",
                    VotesFor = 2789,
                    VotesAgainst = 445,
                    CreatedAt = DateTime.Now.AddDays(-6),
                    ViewsCount = 6789,
                    CreatedByDisplayName = "Paul �ducateur",
                    CreatedById = "user5",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 6,
                    Title = "Remboursement des m�decines douces",
                    Description = "Inclure dans le remboursement de la S�curit� Sociale certaines m�decines alternatives comme l'ost�opathie, l'acupuncture et l'hom�opathie.",
                    CategoryId = 6,
                    CategoryName = "Sant�",
                    CategoryColor = "#e67e22",
                    CategoryIcon = "fas fa-heartbeat",
                    VotesFor = 1543,
                    VotesAgainst = 876,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    ViewsCount = 3456,
                    CreatedByDisplayName = "Dr. Claire Sant�",
                    CreatedById = "user6",
                    Status = ProposalStatus.Active
                }
            };
        }

        private DashboardStatsDto CreateSampleStats()
        {
            return new DashboardStatsDto
            {
                TotalUsers = 15742,
                ActiveUsers = 3856,
                TotalVotes = 48392,
                ActiveProposals = 156,
                TotalProposals = 1247,
                TotalComments = 12847,
                RasLebolMeter = 68.5,
                TopCategories = new List<CategoryStatsDto>
                {
                    new() { CategoryId = 1, CategoryName = "�conomie", CategoryColor = "#e74c3c", CategoryIcon = "fas fa-coins", ProposalCount = 89, VoteCount = 1677 },
                    new() { CategoryId = 2, CategoryName = "Environnement", CategoryColor = "#27ae60", CategoryIcon = "fas fa-leaf", ProposalCount = 76, VoteCount = 1798 },
                    new() { CategoryId = 3, CategoryName = "Social", CategoryColor = "#3498db", CategoryIcon = "fas fa-users", ProposalCount = 134, VoteCount = 2456 },
                    new() { CategoryId = 4, CategoryName = "Num�rique", CategoryColor = "#9b59b6", CategoryIcon = "fas fa-laptop", ProposalCount = 67, VoteCount = 1234 },
                    new() { CategoryId = 5, CategoryName = "�ducation", CategoryColor = "#f39c12", CategoryIcon = "fas fa-graduation-cap", ProposalCount = 112, VoteCount = 1897 },
                    new() { CategoryId = 6, CategoryName = "Sant�", CategoryColor = "#e67e22", CategoryIcon = "fas fa-heartbeat", ProposalCount = 95, VoteCount = 1567 }
                },
                DailyVoteTrends = Enumerable.Range(0, 7).Select(i => new DailyVoteStatsDto
                {
                    Date = DateTime.Today.AddDays(-i),
                    VotesFor = Random.Shared.Next(150, 250),
                    VotesAgainst = Random.Shared.Next(50, 120)
                }).ToList(),
                NicolasLevelDistribution = new List<NicolasLevelStatsDto>
                {
                    new() { Level = ContributionLevel.PetitNicolas, Count = 8945, Percentage = 56.8 },
                    new() { Level = ContributionLevel.GrosMoyenNicolas, Count = 4126, Percentage = 26.2 },
                    new() { Level = ContributionLevel.GrosNicolas, Count = 2034, Percentage = 12.9 },
                    new() { Level = ContributionLevel.NicolasSupreme, Count = 637, Percentage = 4.1 }
                }
            };
        }
    }
}