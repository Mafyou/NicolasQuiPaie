using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieWeb.Services
{
    public class SampleDataService
    {
        private readonly List<ProposalDto> _sampleProposals;
        private readonly List<CategoryDto> _sampleCategories;
        private readonly List<CommentDto> _sampleComments;
        private readonly DashboardStatsDto _sampleStats;

        public SampleDataService()
        {
            _sampleCategories = CreateSampleCategories();
            _sampleProposals = CreateSampleProposals();
            _sampleComments = CreateSampleComments();
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
        /// Récupère les commentaires d'une proposition spécifique
        /// </summary>
        public Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId)
        {
            var comments = _sampleComments
                .Where(c => c.ProposalId == proposalId && c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToList();
            return Task.FromResult<IEnumerable<CommentDto>>(comments);
        }

        /// <summary>
        /// Méthode de test pour vérifier que les données sont générées correctement
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
            logger.LogInformation("Sample Comments Count: {Count}", _sampleComments.Count);
            logger.LogInformation("=========================");
        }

        private List<CategoryDto> CreateSampleCategories()
        {
            return new List<CategoryDto>
            {
                new() { Id = 1, Name = "Économie", Description = "Propositions économiques et fiscales", Color = "#e74c3c", IconClass = "fas fa-coins" },
                new() { Id = 2, Name = "Environnement", Description = "Écologie et développement durable", Color = "#27ae60", IconClass = "fas fa-leaf" },
                new() { Id = 3, Name = "Social", Description = "Questions sociales et sociétales", Color = "#3498db", IconClass = "fas fa-users" },
                new() { Id = 4, Name = "Numérique", Description = "Transformation numérique et tech", Color = "#9b59b6", IconClass = "fas fa-laptop" },
                new() { Id = 5, Name = "Éducation", Description = "Système éducatif et formation", Color = "#f39c12", IconClass = "fas fa-graduation-cap" },
                new() { Id = 6, Name = "Santé", Description = "Système de santé publique", Color = "#e67e22", IconClass = "fas fa-heartbeat" }
            };
        }

        private List<ProposalDto> CreateSampleProposals()
        {
            return new List<ProposalDto>
            {
                new()
                {
                    Id = 1,
                    Title = "Réduction de la TVA sur les produits bio",
                    Description = "Proposition pour réduire la TVA de 20% à 5,5% sur tous les produits alimentaires biologiques certifiés pour encourager une alimentation saine et durable.",
                    CategoryId = 1,
                    CategoryName = "Économie",
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
                    Title = "Interdiction des jets privés pour les trajets courts",
                    Description = "Interdire l'utilisation de jets privés pour tous les trajets de moins de 500km en Europe, avec alternatives en train haute vitesse obligatoires.",
                    CategoryId = 2,
                    CategoryName = "Environnement",
                    CategoryColor = "#27ae60",
                    CategoryIcon = "fas fa-leaf",
                    VotesFor = 2134,
                    VotesAgainst = 432,
                    CreatedAt = DateTime.Now.AddDays(-3),
                    ViewsCount = 5678,
                    CreatedByDisplayName = "Pierre Écolo",
                    CreatedById = "user2",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 3,
                    Title = "Semaine de 4 jours obligatoire",
                    Description = "Instaurer une semaine de travail de 32 heures (4 jours) sans réduction de salaire pour améliorer la qualité de vie et la productivité.",
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
                    Description = "Déployer un réseau WiFi public gratuit et sécurisé dans toutes les communes de France pour réduire la fracture numérique.",
                    CategoryId = 4,
                    CategoryName = "Numérique",
                    CategoryColor = "#9b59b6",
                    CategoryIcon = "fas fa-laptop",
                    VotesFor = 1876,
                    VotesAgainst = 287,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    ViewsCount = 4567,
                    CreatedByDisplayName = "Sophie Connectée",
                    CreatedById = "user4",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 5,
                    Title = "Cours d'éducation financière obligatoire",
                    Description = "Rendre obligatoires les cours d'éducation financière dès le collège pour apprendre la gestion budgétaire, les investissements et les impôts.",
                    CategoryId = 5,
                    CategoryName = "Éducation",
                    CategoryColor = "#f39c12",
                    CategoryIcon = "fas fa-graduation-cap",
                    VotesFor = 2789,
                    VotesAgainst = 445,
                    CreatedAt = DateTime.Now.AddDays(-6),
                    ViewsCount = 6789,
                    CreatedByDisplayName = "Paul Éducateur",
                    CreatedById = "user5",
                    Status = ProposalStatus.Active
                },
                new()
                {
                    Id = 6,
                    Title = "Remboursement des médecines douces",
                    Description = "Inclure dans le remboursement de la Sécurité Sociale certaines médecines alternatives comme l'ostéopathie, l'acupuncture et l'homéopathie.",
                    CategoryId = 6,
                    CategoryName = "Santé",
                    CategoryColor = "#e67e22",
                    CategoryIcon = "fas fa-heartbeat",
                    VotesFor = 1543,
                    VotesAgainst = 876,
                    CreatedAt = DateTime.Now.AddDays(-4),
                    ViewsCount = 3456,
                    CreatedByDisplayName = "Dr. Claire Santé",
                    CreatedById = "user6",
                    Status = ProposalStatus.Active
                }
            };
        }

        private List<CommentDto> CreateSampleComments()
        {
            var comments = new List<CommentDto>();

            // Commentaires pour la proposition 1 (TVA bio)
            comments.AddRange([
                new CommentDto
                {
                    Id = 1,
                    Content = "Excellente idée ! Cela encouragerait vraiment l'achat de produits bio. Mais il faudrait s'assurer que les producteurs répercutent bien cette baisse sur les prix.",
                    CreatedAt = DateTime.Now.AddDays(-4),
                    LikesCount = 23,
                    UserId = "commenter1",
                    UserDisplayName = "Lucas Vert",
                    ProposalId = 1,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = [
                        new CommentDto
                        {
                            Id = 2,
                            Content = "Tout à fait d'accord ! Un mécanisme de contrôle des prix serait nécessaire pour éviter que les distributeurs gardent la marge.",
                            CreatedAt = DateTime.Now.AddDays(-4).AddHours(2),
                            LikesCount = 8,
                            UserId = "commenter2",
                            UserDisplayName = "Anna Consommatrice",
                            ProposalId = 1,
                            ParentCommentId = 1,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        }
                    ]
                },
                new CommentDto
                {
                    Id = 3,
                    Content = "Je suis mitigé... Cette mesure va coûter cher à l'État. Ne vaudrait-il pas mieux investir directement dans l'agriculture bio ?",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    LikesCount = 15,
                    UserId = "commenter3",
                    UserDisplayName = "Thomas Prudent",
                    ProposalId = 1,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                }
            ]);

            // Commentaires pour la proposition 2 (Jets privés)
            comments.AddRange([
                new CommentDto
                {
                    Id = 4,
                    Content = "Mesure indispensable pour le climat ! Les jets privés émettent 40 fois plus de CO2 par passager qu'un vol commercial. C'est inacceptable en 2025.",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    LikesCount = 67,
                    UserId = "commenter4",
                    UserDisplayName = "Émilie Climat",
                    ProposalId = 2,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = [
                        new CommentDto
                        {
                            Id = 5,
                            Content = "Les chiffres sont parlants ! Et en plus, développer le train haute vitesse créerait des emplois.",
                            CreatedAt = DateTime.Now.AddDays(-2).AddHours(3),
                            LikesCount = 12,
                            UserId = "commenter5",
                            UserDisplayName = "Marc Ferroviaire",
                            ProposalId = 2,
                            ParentCommentId = 4,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        }
                    ]
                },
                new CommentDto
                {
                    Id = 6,
                    Content = "Attention à ne pas pénaliser les entreprises françaises. Il faut que cette règle s'applique à toute l'Europe en même temps.",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    LikesCount = 34,
                    UserId = "commenter6",
                    UserDisplayName = "Philippe Économiste",
                    ProposalId = 2,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                }
            ]);

            // Commentaires pour la proposition 3 (Semaine 4 jours)
            comments.AddRange([
                new CommentDto
                {
                    Id = 7,
                    Content = "Enfin ! Des études montrent que la productivité augmente avec moins d'heures. Le Danemark et la Suède l'ont prouvé. Vivement en France ! 🇫🇷",
                    CreatedAt = DateTime.Now.AddDays(-6),
                    LikesCount = 89,
                    UserId = "commenter7",
                    UserDisplayName = "Sarah Moderne",
                    ProposalId = 3,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = [
                        new CommentDto
                        {
                            Id = 8,
                            Content = "Exact ! Et moins de stress = moins de dépenses de santé pour la collectivité. C'est gagnant-gagnant !",
                            CreatedAt = DateTime.Now.AddDays(-6).AddHours(1),
                            LikesCount = 24,
                            UserId = "commenter8",
                            UserDisplayName = "Dr. Julien Santé",
                            ProposalId = 3,
                            ParentCommentId = 7,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        },
                        new CommentDto
                        {
                            Id = 9,
                            Content = "Mais comment faire pour les services publics ? Les hôpitaux ne peuvent pas fermer un jour par semaine...",
                            CreatedAt = DateTime.Now.AddDays(-5),
                            LikesCount = 18,
                            UserId = "commenter9",
                            UserDisplayName = "Infirmière Claire",
                            ProposalId = 3,
                            ParentCommentId = 7,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        }
                    ]
                },
                new CommentDto
                {
                    Id = 10,
                    Content = "Je suis chef d'entreprise et franchement inquiet. Nos concurrents européens travaillent plus. Comment rester compétitifs ?",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    LikesCount = 42,
                    UserId = "commenter10",
                    UserDisplayName = "Michel Entreprise",
                    ProposalId = 3,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                }
            ]);

            // Commentaires pour la proposition 4 (WiFi public)
            comments.AddRange([
                new CommentDto
                {
                    Id = 11,
                    Content = "Super initiative ! En tant qu'étudiant, c'est galère quand on a plus de 4G. Le WiFi public gratuit, c'est la base en 2025 !",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    LikesCount = 56,
                    UserId = "commenter11",
                    UserDisplayName = "Maxime Étudiant",
                    ProposalId = 4,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                },
                new CommentDto
                {
                    Id = 12,
                    Content = "Bonne idée mais attention à la sécurité ! Il faut que ce soit vraiment sécurisé, pas comme certains WiFi publics actuels.",
                    CreatedAt = DateTime.Now.AddHours(-18),
                    LikesCount = 31,
                    UserId = "commenter12",
                    UserDisplayName = "Alex Sécurité",
                    ProposalId = 4,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                }
            ]);

            // Commentaires pour la proposition 5 (Éducation financière)
            comments.AddRange([
                new CommentDto
                {
                    Id = 13,
                    Content = "ENFIN ! Je ne savais rien des impôts, crédits, épargne en sortant du lycée. Mes parents non plus d'ailleurs... Cette matière devrait être obligatoire !",
                    CreatedAt = DateTime.Now.AddDays(-5),
                    LikesCount = 78,
                    UserId = "commenter13",
                    UserDisplayName = "Camille Jeune",
                    ProposalId = 5,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = [
                        new CommentDto
                        {
                            Id = 14,
                            Content = "Pareil pour moi ! J'ai découvert le PEA à 30 ans... Si j'avais su plus tôt !",
                            CreatedAt = DateTime.Now.AddDays(-4),
                            LikesCount = 19,
                            UserId = "commenter14",
                            UserDisplayName = "Romain Épargne",
                            ProposalId = 5,
                            ParentCommentId = 13,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        }
                    ]
                },
                new CommentDto
                {
                    Id = 15,
                    Content = "Excellente proposition ! En tant que prof, je vois trop d'élèves perdus sur ces sujets. Il faut 2h/semaine minimum.",
                    CreatedAt = DateTime.Now.AddDays(-4),
                    LikesCount = 45,
                    UserId = "commenter15",
                    UserDisplayName = "Prof Hélène",
                    ProposalId = 5,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                }
            ]);

            // Commentaires pour la proposition 6 (Médecines douces)
            comments.AddRange([
                new CommentDto
                {
                    Id = 16,
                    Content = "Très bonne idée ! L'ostéopathie m'a sauvé le dos, mais à 60€ la séance, tout le monde ne peut pas se soigner.",
                    CreatedAt = DateTime.Now.AddDays(-3),
                    LikesCount = 41,
                    UserId = "commenter16",
                    UserDisplayName = "Martine Dos",
                    ProposalId = 6,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = []
                },
                new CommentDto
                {
                    Id = 17,
                    Content = "Attention à l'homéopathie... Il faut des preuves scientifiques solides avant de rembourser. L'ostéopathie et l'acupuncture, OK, mais restons rationnels !",
                    CreatedAt = DateTime.Now.AddDays(-2),
                    LikesCount = 63,
                    UserId = "commenter17",
                    UserDisplayName = "Dr. Rationaliste",
                    ProposalId = 6,
                    ParentCommentId = null,
                    IsDeleted = false,
                    IsModerated = false,
                    Replies = [
                        new CommentDto
                        {
                            Id = 18,
                            Content = "D'accord pour l'homéopathie, mais l'acupuncture a fait ses preuves pour la douleur. L'OMS la reconnaît !",
                            CreatedAt = DateTime.Now.AddDays(-1),
                            LikesCount = 22,
                            UserId = "commenter18",
                            UserDisplayName = "Acupuncteur Paul",
                            ProposalId = 6,
                            ParentCommentId = 17,
                            IsDeleted = false,
                            IsModerated = false,
                            Replies = []
                        }
                    ]
                }
            ]);

            return comments;
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
                    new() { CategoryId = 1, CategoryName = "Économie", CategoryColor = "#e74c3c", CategoryIcon = "fas fa-coins", ProposalCount = 89, VoteCount = 1677 },
                    new() { CategoryId = 2, CategoryName = "Environnement", CategoryColor = "#27ae60", CategoryIcon = "fas fa-leaf", ProposalCount = 76, VoteCount = 1798 },
                    new() { CategoryId = 3, CategoryName = "Social", CategoryColor = "#3498db", CategoryIcon = "fas fa-users", ProposalCount = 134, VoteCount = 2456 },
                    new() { CategoryId = 4, CategoryName = "Numérique", CategoryColor = "#9b59b6", CategoryIcon = "fas fa-laptop", ProposalCount = 67, VoteCount = 1234 },
                    new() { CategoryId = 5, CategoryName = "Éducation", CategoryColor = "#f39c12", CategoryIcon = "fas fa-graduation-cap", ProposalCount = 112, VoteCount = 1897 },
                    new() { CategoryId = 6, CategoryName = "Santé", CategoryColor = "#e67e22", CategoryIcon = "fas fa-heartbeat", ProposalCount = 95, VoteCount = 1567 }
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