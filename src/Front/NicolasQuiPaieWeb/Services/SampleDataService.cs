namespace NicolasQuiPaieWeb.Services;

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

        if (!string.IsNullOrEmpty(category) && int.TryParse(category, out int categoryId))
        {
            filtered = filtered.Where(p => p.CategoryId == categoryId);
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

    public Task<IEnumerable<ProposalDto>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
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

        var result = filtered
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
        
        return Task.FromResult<IEnumerable<ProposalDto>>(result);
    }

    public Task<IEnumerable<ProposalDto>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
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

        var result = filtered
            .OrderByDescending(p => p.VotesFor)
            .ThenByDescending(p => p.TotalVotes)
            .ThenByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
        
        return Task.FromResult<IEnumerable<ProposalDto>>(result);
    }

    public Task<IEnumerable<ProposalDto>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        var filtered = _sampleProposals.AsEnumerable()
            .Where(p => p.VotesFor > 0 && p.VotesAgainst > 0); // Only proposals with both types of votes

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

        // Order by controversy score: closer to 50% ratio = more controversial
        var result = filtered
            .OrderBy(p => Math.Abs(0.5 - p.ApprovalRate / 100.0))
            .ThenByDescending(p => p.TotalVotes)
            .ThenByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();
        
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
        logger.LogWarning("=== SAMPLE DATA DEBUG ===");
        logger.LogWarning("TopCategories Count: {Count}", _sampleStats.TopCategories.Count);
        foreach (var category in _sampleStats.TopCategories)
        {
            logger.LogWarning("Category: {Name}, VoteCount: {VoteCount}, Color: {Color}",
                category.CategoryName, category.VoteCount, category.CategoryColor);
        }
        logger.LogWarning("NicolasLevelDistribution Count: {Count}", _sampleStats.NicolasLevelDistribution.Count);
        foreach (var level in _sampleStats.NicolasLevelDistribution)
        {
            logger.LogWarning("Level: {Level}, Count: {Count}, Percentage: {Percentage}%",
                level.Level, level.Count, level.Percentage);
        }
        logger.LogWarning("Sample Comments Count: {Count}", _sampleComments.Count);
        logger.LogWarning("=========================");
    }

    private List<CategoryDto> CreateSampleCategories()
    {
        return new List<CategoryDto>
        {
            new() { Id = 1, Name = "Régalien", Description = "Fonctions de souveraineté : police, justice, défense", Color = "#002E5D", IconClass = "fa-solid fa-gavel" },
            new() { Id = 2, Name = "Économie", Description = "Fiscalité, compétitivité, marché libre", Color = "#007ACC", IconClass = "fa-solid fa-chart-line" },
            new() { Id = 3, Name = "Gouvernance", Description = "Transparence, lutte contre la corruption", Color = "#5A189A", IconClass = "fa-solid fa-scale-balanced" },
            new() { Id = 4, Name = "Social", Description = "Aides, redistribution, travail", Color = "#A4133C", IconClass = "fa-solid fa-hand-holding-heart" },
            new() { Id = 5, Name = "Identité", Description = "Culture, langue, cohésion nationale", Color = "#D7263D", IconClass = "fa-solid fa-flag" },
            new() { Id = 6, Name = "Retraites", Description = "Financement et équité inter-générationnelle", Color = "#5CC4E0", IconClass = "fa-solid fa-person-cane" },
            new() { Id = 7, Name = "Santé", Description = "Organisation, efficacité, assurances", Color = "#198754", IconClass = "fa-solid fa-stethoscope" },
            new() { Id = 8, Name = "Éducation", Description = "Programmes, carte scolaire, valeurs", Color = "#FF9F1C", IconClass = "fa-solid fa-school" },
            new() { Id = 9, Name = "Institutions", Description = "Processus législatif et contre-pouvoirs", Color = "#3C096C", IconClass = "fa-solid fa-landmark" },
            new() { Id = 10, Name = "Europe", Description = "Relations et souveraineté vis-à-vis de l'UE", Color = "#003399", IconClass = "fa-brands fa-eu" },
            new() { Id = 11, Name = "Immigration", Description = "Contrôle des flux et intégration", Color = "#8E2A2A", IconClass = "fa-solid fa-passport" }
        };
    }

    private List<ProposalDto> CreateSampleProposals()
    {
        return new List<ProposalDto>
        {
            new()
            {
                Id = 1,
                Title = "ÉTAT RECENTRÉ SUR L'ESSENTIEL",
                Description = "Police, justice et défense sont abandonnées aujourd'hui, laissant insécurité et chaos prospérer. Nous voulons supprimer les structures et dépenses inutiles pour renforcer le régalien et protéger les Français, sans gaspiller un centime ailleurs.",
                CategoryId = 1,
                CategoryName = "Régalien",
                CategoryColor = "#002E5D",
                CategoryIcon = "fa-solid fa-gavel",
                VotesFor = 2847,
                VotesAgainst = 456,
                CreatedAt = DateTime.Now.AddDays(-12),
                ViewsCount = 8420,
                CreatedByDisplayName = "Nicolas Souverain",
                CreatedById = "user1",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 2,
                Title = "LIBERTÉ ÉCONOMIQUE PURE",
                Description = "Moins de taxes, moins de règles. Rendez aux actifs leur salaire et aux entreprises leur compétitivité.",
                CategoryId = 2,
                CategoryName = "Économie",
                CategoryColor = "#007ACC",
                CategoryIcon = "fa-solid fa-chart-line",
                VotesFor = 3142,
                VotesAgainst = 789,
                CreatedAt = DateTime.Now.AddDays(-10),
                ViewsCount = 9876,
                CreatedByDisplayName = "Marie Libérale",
                CreatedById = "user2",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 3,
                Title = "ZÉRO SUBVENTION, ZÉRO PRIVILÈGE",
                Description = "Supprimez toutes les subventions, aides aux entreprises, niches, exonérations et réductions fiscales. Que chacun joue à armes égales !",
                CategoryId = 2,
                CategoryName = "Économie",
                CategoryColor = "#007ACC",
                CategoryIcon = "fa-solid fa-chart-line",
                VotesFor = 2156,
                VotesAgainst = 1234,
                CreatedAt = DateTime.Now.AddDays(-8),
                ViewsCount = 6789,
                CreatedByDisplayName = "Pierre Équité",
                CreatedById = "user3",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 4,
                Title = "NON AU CAPITALISME DE CONNIVENCE",
                Description = "Stop aux magouilles entre élites et entreprises. Fini les privilèges pour les copains du pouvoir !",
                CategoryId = 3,
                CategoryName = "Gouvernance",
                CategoryColor = "#5A189A",
                CategoryIcon = "fa-solid fa-scale-balanced",
                VotesFor = 3567,
                VotesAgainst = 234,
                CreatedAt = DateTime.Now.AddDays(-6),
                ViewsCount = 12345,
                CreatedByDisplayName = "Sophie Justice",
                CreatedById = "user4",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 5,
                Title = "FIN DE L'ASSISTANAT",
                Description = "Basta les aides sans contreparties qui tuent l'envie de bosser. Priorité à ceux qui contribuent.",
                CategoryId = 4,
                CategoryName = "Social",
                CategoryColor = "#A4133C",
                CategoryIcon = "fa-solid fa-hand-holding-heart",
                VotesFor = 2234,
                VotesAgainst = 1567,
                CreatedAt = DateTime.Now.AddDays(-5),
                ViewsCount = 7890,
                CreatedByDisplayName = "Jean Travailleur",
                CreatedById = "user5",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 6,
                Title = "IDENTITÉ, SOCLE DE PROSPÉRITÉ",
                Description = "Une France unie par sa culture, sa langue et ses traditions produit mieux. Protégeons ce qui nous rend forts et évacuons ce qui nous fait régresser.",
                CategoryId = 5,
                CategoryName = "Identité",
                CategoryColor = "#D7263D",
                CategoryIcon = "fa-solid fa-flag",
                VotesFor = 2789,
                VotesAgainst = 678,
                CreatedAt = DateTime.Now.AddDays(-4),
                ViewsCount = 9123,
                CreatedByDisplayName = "Paul Tradition",
                CreatedById = "user6",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 7,
                Title = "DES RETRAITES QUI PENSENT À LA JEUNESSE",
                Description = "Stop aux privilèges. Les actifs ne doivent plus financer des régimes spéciaux ni des retraites géantes pour une minorité. Instaurons un système équitable tourné vers l'avenir.",
                CategoryId = 6,
                CategoryName = "Retraites",
                CategoryColor = "#5CC4E0",
                CategoryIcon = "fa-solid fa-person-cane",
                VotesFor = 3234,
                VotesAgainst = 456,
                CreatedAt = DateTime.Now.AddDays(-3),
                ViewsCount = 11567,
                CreatedByDisplayName = "Camille Jeune",
                CreatedById = "user7",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 8,
                Title = "SANTÉ PRAGMATIQUE",
                Description = "Efficacité et liberté. Réduisons l'État dans la santé, favorisons la concurrence et le libre choix avec des assurances accessibles.",
                CategoryId = 7,
                CategoryName = "Santé",
                CategoryColor = "#198754",
                CategoryIcon = "fa-solid fa-stethoscope",
                VotesFor = 1987,
                VotesAgainst = 1123,
                CreatedAt = DateTime.Now.AddDays(-2),
                ViewsCount = 6456,
                CreatedByDisplayName = "Dr. Claire Libre",
                CreatedById = "user8",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 9,
                Title = "ÉDUCATION CENTRÉE SUR L'ESSENTIEL",
                Description = "L'école doit transmettre notre histoire, notre langue et nos valeurs, pas des idéologies hors sol. Suppression de la carte scolaire et mise en concurrence pour davantage de liberté parentale et moins de bureaucratie.",
                CategoryId = 8,
                CategoryName = "Éducation",
                CategoryColor = "#FF9F1C",
                CategoryIcon = "fa-solid fa-school",
                VotesFor = 2567,
                VotesAgainst = 789,
                CreatedAt = DateTime.Now.AddDays(-1),
                ViewsCount = 8234,
                CreatedByDisplayName = "Hélène École",
                CreatedById = "user9",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 10,
                Title = "FIN DU BLOCAGE LÉGISLATIF",
                Description = "Certaines structures étatiques bloquent les lois dont les Français ont besoin et masquent les réalités économiques et sociales. Réformons-les pour qu'elles servent le peuple, pas une caste.",
                CategoryId = 9,
                CategoryName = "Institutions",
                CategoryColor = "#3C096C",
                CategoryIcon = "fa-solid fa-landmark",
                VotesFor = 2345,
                VotesAgainst = 567,
                CreatedAt = DateTime.Now.AddHours(-18),
                ViewsCount = 7123,
                CreatedByDisplayName = "Marc Réforme",
                CreatedById = "user10",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 11,
                Title = "RAPPORT DE FORCE AVEC L'UE",
                Description = "Basta les diktats de Bruxelles qui étouffent nos entreprises et nos travailleurs. Exigeons un véritable rapport de force pour défendre nos intérêts.",
                CategoryId = 10,
                CategoryName = "Europe",
                CategoryColor = "#003399",
                CategoryIcon = "fa-brands fa-eu",
                VotesFor = 2789,
                VotesAgainst = 456,
                CreatedAt = DateTime.Now.AddHours(-12),
                ViewsCount = 8567,
                CreatedByDisplayName = "Anna Souveraine",
                CreatedById = "user11",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 12,
                Title = "FIN DE L'IMMIGRATION DE MASSE",
                Description = "Au-delà de la fin des aides sociales et de la préservation de notre identité, demandons l'arrêt des mécanismes de régulation trop permissifs. Exigeons également l'expulsion des étrangers clandestins, délinquants ou inactifs de longue durée.",
                CategoryId = 11,
                CategoryName = "Immigration",
                CategoryColor = "#8E2A2A",
                CategoryIcon = "fa-solid fa-passport",
                VotesFor = 3456,
                VotesAgainst = 1234,
                CreatedAt = DateTime.Now.AddHours(-6),
                ViewsCount = 13456,
                CreatedByDisplayName = "Thomas Contrôle",
                CreatedById = "user12",
                Status = ProposalStatus.Active
            },
            new()
            {
                Id = 13,
                Title = "REFUS DES RÉCUPÉRATIONS CONTRE-NATURE",
                Description = "Nous rejetons les récupérations politiques de la gauche ou des partis étatistes opposés à ce manifeste ainsi que les idéologies contraires à la liberté et à l'identité françaises.",
                CategoryId = 5,
                CategoryName = "Identité",
                CategoryColor = "#D7263D",
                CategoryIcon = "fa-solid fa-flag",
                VotesFor = 1987,
                VotesAgainst = 654,
                CreatedAt = DateTime.Now.AddHours(-2),
                ViewsCount = 5678,
                CreatedByDisplayName = "Émilie Résistance",
                CreatedById = "user13",
                Status = ProposalStatus.Active
            }
        };
    }

    private List<CommentDto> CreateSampleComments()
    {
        var comments = new List<CommentDto>();

        // Commentaires pour la proposition 1 (État recentré)
        comments.AddRange([
            new CommentDto
            {
                Id = 1,
                Content = "Enfin quelqu'un qui dit la vérité ! L'État gaspille des milliards dans des trucs inutiles pendant que nos policiers manquent de moyens. Il faut vraiment se recentrer sur l'essentiel !",
                CreatedAt = DateTime.Now.AddDays(-11),
                LikesCount = 47,
                UserId = "commenter1",
                UserDisplayName = "Michel Réaliste",
                ProposalId = 1,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = [
                    new CommentDto
                    {
                        Id = 2,
                        Content = "Exactement ! Et nos militaires aussi. On a l'armée la plus faible d'Europe à cause de ces priorités à l'envers.",
                        CreatedAt = DateTime.Now.AddDays(-11).AddHours(2),
                        LikesCount = 23,
                        UserId = "commenter2",
                        UserDisplayName = "Capitaine Défense",
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
                Content = "Mais attention à ne pas tout casser non plus. Il faut des services publics qui fonctionnent, pas juste du sécuritaire.",
                CreatedAt = DateTime.Now.AddDays(-10),
                LikesCount = 18,
                UserId = "commenter3",
                UserDisplayName = "Julie Modérée",
                ProposalId = 1,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = []
            }
        ]);

        // Commentaires pour la proposition 2 (Liberté économique)
        comments.AddRange([
            new CommentDto
            {
                Id = 4,
                Content = "100% d'accord ! Je bosse 50h/semaine et je vois la moitié partir en impôts. Laissez-nous respirer ! Plus de liberté économique = plus d'emplois.",
                CreatedAt = DateTime.Now.AddDays(-9),
                LikesCount = 89,
                UserId = "commenter4",
                UserDisplayName = "Entrepreneur Libre",
                ProposalId = 2,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = [
                    new CommentDto
                    {
                        Id = 5,
                        Content = "Pareil pour moi ! En tant qu'artisan, je passe plus de temps à faire de la paperasse qu'à bosser. C'est dingue !",
                        CreatedAt = DateTime.Now.AddDays(-9).AddHours(3),
                        LikesCount = 34,
                        UserId = "commenter5",
                        UserDisplayName = "Artisan Français",
                        ProposalId = 2,
                        ParentCommentId = 4,
                        IsDeleted = false,
                        IsModerated = false,
                        Replies = []
                    }
                ]
            }
        ]);

        // Commentaires pour la proposition 4 (Capitalisme de connivence)
        comments.AddRange([
            new CommentDto
            {
                Id = 6,
                Content = "Enfin ! Marre de voir les gros se servir pendant que les PME crèvent. Il faut arrêter ce système de copains-coquins !",
                CreatedAt = DateTime.Now.AddDays(-5),
                LikesCount = 156,
                UserId = "commenter6",
                UserDisplayName = "Petite Entreprise",
                ProposalId = 4,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = [
                    new CommentDto
                    {
                        Id = 7,
                        Content = "Exactement ! Le CAC 40 a ses entrées à l'Élysée, nous on a que dalle. Il faut de l'égalité de traitement !",
                        CreatedAt = DateTime.Now.AddDays(-5).AddHours(1),
                        LikesCount = 67,
                        UserId = "commenter7",
                        UserDisplayName = "Patron PME",
                        ProposalId = 4,
                        ParentCommentId = 6,
                        IsDeleted = false,
                        IsModerated = false,
                        Replies = []
                    }
                ]
            }
        ]);

        // Commentaires pour la proposition 7 (Retraites)
        comments.AddRange([
            new CommentDto
            {
                Id = 8,
                Content = "MERCI ! J'ai 25 ans et je cotise pour des régimes spéciaux de privilégiés qui partent à 50 ans. Pendant ce temps, moi je vais bosser jusqu'à 67 ans minimum. C'est du vol organisé ! 😡",
                CreatedAt = DateTime.Now.AddDays(-2),
                LikesCount = 234,
                UserId = "commenter8",
                UserDisplayName = "Jeune Actif",
                ProposalId = 7,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = [
                    new CommentDto
                    {
                        Id = 9,
                        Content = "Pareil ! On finance les retraites dorées des autres pendant qu'on n'aura rien. Il faut de l'équité !",
                        CreatedAt = DateTime.Now.AddDays(-2).AddHours(2),
                        LikesCount = 78,
                        UserId = "commenter9",
                        UserDisplayName = "Génération Spoliée",
                        ProposalId = 7,
                        ParentCommentId = 8,
                        IsDeleted = false,
                        IsModerated = false,
                        Replies = []
                    }
                ]
            }
        ]);

        // Commentaires pour la proposition 9 (Éducation)
        comments.AddRange([
            new CommentDto
            {
                Id = 10,
                Content = "En tant que prof, je suis d'accord ! On nous impose des programmes idéologiques au lieu d'enseigner les bases. Nos élèves ne savent plus écrire mais connaissent la théorie du genre... 🤦‍♀️",
                CreatedAt = DateTime.Now.AddHours(-20),
                LikesCount = 145,
                UserId = "commenter10",
                UserDisplayName = "Prof Désabusée",
                ProposalId = 9,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = [
                    new CommentDto
                    {
                        Id = 11,
                        Content = "Merci ! En tant que parent, je vois bien que l'école n'enseigne plus ce qui compte vraiment. Vive la liberté scolaire !",
                        CreatedAt = DateTime.Now.AddHours(-18),
                        LikesCount = 67,
                        UserId = "commenter11",
                        UserDisplayName = "Parent Concerné",
                        ProposalId = 9,
                        ParentCommentId = 10,
                        IsDeleted = false,
                        IsModerated = false,
                        Replies = []
                    }
                ]
            }
        ]);

        // Commentaires pour la proposition 12 (Immigration)
        comments.AddRange([
            new CommentDto
            {
                Id = 12,
                Content = "Enfin quelqu'un qui ose dire les choses ! L'immigration de masse coûte une fortune et détruit notre cohésion sociale. Il faut du pragmatisme, pas de l'idéologie !",
                CreatedAt = DateTime.Now.AddHours(-4),
                LikesCount = 189,
                UserId = "commenter12",
                UserDisplayName = "Citoyen Lucide",
                ProposalId = 12,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = []
            },
            new CommentDto
            {
                Id = 13,
                Content = "Il faut surtout plus de contrôles et moins d'assistanat. Ceux qui veulent s'intégrer et travailler, OK. Les autres, dehors !",
                CreatedAt = DateTime.Now.AddHours(-3),
                LikesCount = 123,
                UserId = "commenter13",
                UserDisplayName = "Français Pragmatique",
                ProposalId = 12,
                ParentCommentId = null,
                IsDeleted = false,
                IsModerated = false,
                Replies = []
            }
        ]);

        return comments;
    }

    private DashboardStatsDto CreateSampleStats()
    {
        return new DashboardStatsDto
        {
            TotalUsers = 23847,
            ActiveUsers = 6234,
            TotalVotes = 67392,
            ActiveProposals = 189,
            TotalProposals = 1456,
            TotalComments = 18947,
            RasLebolMeter = 78.3, // Plus élevé pour refléter l'esprit des propositions
            TopCategories =
            [
                new() { CategoryId = 2, CategoryName = "Économie", CategoryColor = "#007ACC", CategoryIcon = "fa-solid fa-chart-line", ProposalCount = 156, VoteCount = 2847 },
                new() { CategoryId = 1, CategoryName = "Régalien", CategoryColor = "#002E5D", CategoryIcon = "fa-solid fa-gavel", ProposalCount = 134, VoteCount = 2456 },
                new() { CategoryId = 5, CategoryName = "Identité", CategoryColor = "#D7263D", CategoryIcon = "fa-solid fa-flag", ProposalCount = 123, VoteCount = 2234 },
                new() { CategoryId = 11, CategoryName = "Immigration", CategoryColor = "#8E2A2A", CategoryIcon = "fa-solid fa-passport", ProposalCount = 98, VoteCount = 1987 },
                new() { CategoryId = 3, CategoryName = "Gouvernance", CategoryColor = "#5A189A", CategoryIcon = "fa-solid fa-scale-balanced", ProposalCount = 87, VoteCount = 1789 },
                new() { CategoryId = 6, CategoryName = "Retraites", CategoryColor = "#5CC4E0", CategoryIcon = "fa-solid fa-person-cane", ProposalCount = 76, VoteCount = 1567 }
            ],
            DailyVoteTrends = [.. Enumerable.Range(0, 7).Select(i => new DailyVoteStatsDto
            {
                Date = DateTime.Today.AddDays(-i),
                VotesFor = Random.Shared.Next(200, 350), // Plus de votes positifs
                VotesAgainst = Random.Shared.Next(30, 80)  // Moins de votes négatifs
            })],
            NicolasLevelDistribution =
            [
                new() { Level = ContributionLevel.PetitNicolas, Count = 12456, Percentage = 52.3 },
                new() { Level = ContributionLevel.GrosMoyenNicolas, Count = 6789, Percentage = 28.5 },
                new() { Level = ContributionLevel.GrosNicolas, Count = 3234, Percentage = 13.6 },
                new() { Level = ContributionLevel.NicolasSupreme, Count = 1368, Percentage = 5.6 }
            ]
        };
    }
}