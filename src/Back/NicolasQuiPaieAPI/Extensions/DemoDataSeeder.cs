namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Sample proposals, votes and comments for the demo site (App Service, the /demo presenter mode), so the tour shows a
/// living platform instead of empty lists. Runs only when <c>SeedDemoData</c> is true (set by the App Service deployment)
/// and only on a database without any proposal: real data is never touched.
/// The votes are real rows of <see cref="Vote"/> cast by a few demo citizens: the API recounts from them, so counters
/// written by hand would be reset at the first vote.
/// </summary>
public static class DemoDataSeeder
{
    public const string SettingKey = "SeedDemoData";

    private readonly record struct DemoCitizen(string Email, string DisplayName, Infrastructure.Models.ContributionLevel Level);

    private readonly record struct DemoProposal(string Title, string Description, string CategoryName, string Tags, int DaysAgo, bool Featured);

    private static readonly IReadOnlyList<DemoCitizen> Citizens =
    [
        new("lea.demo@nicolasquipaie.invalid", "Léa, Petit Nicolas", Infrastructure.Models.ContributionLevel.PetitNicolas),
        new("karim.demo@nicolasquipaie.invalid", "Karim, Gros Moyen Nicolas", Infrastructure.Models.ContributionLevel.GrosMoyenNicolas),
        new("sophie.demo@nicolasquipaie.invalid", "Sophie, Gros Nicolas", Infrastructure.Models.ContributionLevel.GrosNicolas),
        new("marc.demo@nicolasquipaie.invalid", "Marc, Gros Nicolas", Infrastructure.Models.ContributionLevel.GrosNicolas),
        new("claire.demo@nicolasquipaie.invalid", "Claire, Nicolas Suprême", Infrastructure.Models.ContributionLevel.NicolasSupreme),
        new("hugo.demo@nicolasquipaie.invalid", "Hugo, Petit Nicolas", Infrastructure.Models.ContributionLevel.PetitNicolas)
    ];

    private static readonly IReadOnlyList<DemoProposal> Proposals =
    [
        new("Publier le coût de chaque nouvelle loi",
            "Chaque projet de loi serait accompagné d'une fiche claire : combien il coûte, qui le paie, et sur combien d'années. Une page, des chiffres, pas de jargon.",
            "Dépenses Publiques", "transparence,budget", 2, true),
        new("Un simulateur d'impôt lisible pour chaque foyer",
            "Un outil en ligne qui montre, ligne par ligne, où part l'impôt payé par un foyer : santé, éducation, dette, défense… en euros, pas en pourcentages.",
            "Fiscalité", "impôts,pédagogie", 4, true),
        new("Les comptes des collectivités ouverts en temps réel",
            "Communes, départements et régions publient leurs dépenses au fil de l'eau, dans un format ouvert que chacun peut consulter et réutiliser.",
            "Services Publics", "open data,collectivités", 6, false),
        new("Des guichets publics ouverts le samedi matin",
            "Mairies, caisses et préfectures ouvertes le samedi matin, pour ceux qui travaillent en semaine et ne peuvent pas poser une demi-journée.",
            "Services Publics", "accessibilité", 9, false),
        new("Un plan de rénovation des écoles sur dix ans",
            "Un calendrier public et financé pour rénover les bâtiments scolaires : isolation, chauffage, sanitaires, école par école.",
            "Éducation", "écoles,rénovation", 12, false),
        new("Réduire les délais des rendez-vous médicaux",
            "Des créneaux réservés aux urgences non vitales dans chaque maison de santé, et un suivi public des délais d'attente par territoire.",
            "Santé", "santé,délais", 15, false),
        new("Entretenir les routes avant d'en construire de nouvelles",
            "Priorité budgétaire à l'entretien du réseau existant : ponts, chaussées, signalisation, avant tout nouveau chantier.",
            "Infrastructure", "routes,entretien", 20, false)
    ];

    // For each proposal, the vote of each citizen in order (true = for, false = against, null = abstains).
    private static readonly IReadOnlyList<bool?[]> Ballots =
    [
        [true, true, true, true, true, false],
        [true, true, false, true, true, true],
        [true, null, true, true, false, true],
        [true, false, true, null, false, true],
        [true, true, true, false, null, true],
        [false, true, true, true, true, null],
        [true, false, null, true, false, false]
    ];

    private static readonly IReadOnlyList<(int Proposal, int Citizen, string Content)> Comments =
    [
        (0, 2, "Enfin ! On saurait ce qu'on paie avant de le payer."),
        (0, 5, "Il faudrait aussi le coût réel après deux ans, pas seulement l'estimation."),
        (1, 1, "Voir la ligne « intérêts de la dette » ferait un choc à beaucoup de monde."),
        (3, 4, "Le samedi, oui, mais sans fermer un autre jour de la semaine."),
        (6, 3, "Un pont qui tient vaut mieux qu'un rond-point de plus.")
    ];

    /// <summary>Adds the demo data when <c>SeedDemoData</c> is on and there is no proposal yet.</summary>
    public static async Task SeedAsync(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        Microsoft.Extensions.Logging.ILogger logger, CancellationToken stoppingToken = default)
    {
        if (!configuration.GetValue<bool>(SettingKey)) return;
        if (await context.Proposals.AnyAsync(stoppingToken))
        {
            logger.LogWarning("Demo data skipped: the database already holds proposals");
            return;
        }

        var categories = await context.Categories.ToDictionaryAsync(category => category.Name, category => category.Id, stoppingToken);
        List<ApplicationUser> citizens = [];
        foreach (var citizen in Citizens)
        {
            var user = await userManager.FindByEmailAsync(citizen.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = citizen.Email,
                    Email = citizen.Email,
                    EmailConfirmed = true,
                    DisplayName = citizen.DisplayName,
                    ContributionLevel = citizen.Level
                };
                // Nobody signs in with these accounts: a random password nobody knows.
                var created = await userManager.CreateAsync(user, $"Demo-{Guid.NewGuid():N}!Aa1");
                if (!created.Succeeded)
                {
                    logger.LogWarning("Demo citizen {Email} not created: {Errors}", citizen.Email, string.Join(", ", created.Errors.Select(error => error.Description)));
                    return;
                }
            }
            citizens.Add(user);
        }

        var now = DateTime.UtcNow;
        List<Proposal> proposals = [];
        foreach (var (index, demo) in Proposals.Index())
        {
            if (!categories.TryGetValue(demo.CategoryName, out var categoryId)) continue;
            var ballots = Ballots[index];
            List<Vote> votes = [];
            foreach (var (position, ballot) in ballots.Index())
            {
                if (ballot is not { } isFor) continue;
                votes.Add(new Vote
                {
                    UserId = citizens[position].Id,
                    VoteType = isFor ? VoteType.For : VoteType.Against,
                    VotedAt = now.AddDays(-demo.DaysAgo).AddHours(position + 1)
                });
            }

            proposals.Add(new Proposal
            {
                Title = demo.Title,
                Description = demo.Description,
                Status = ProposalStatus.Active,
                CreatedAt = now.AddDays(-demo.DaysAgo),
                CategoryId = categoryId,
                CreatedById = citizens[index % citizens.Count].Id,
                Tags = demo.Tags,
                IsFeatured = demo.Featured,
                ViewsCount = 40 + (Proposals.Count - index) * 17,
                VotesFor = votes.Count(vote => vote.VoteType == VoteType.For),
                VotesAgainst = votes.Count(vote => vote.VoteType == VoteType.Against),
                Votes = votes
            });
        }

        context.Proposals.AddRange(proposals);
        await context.SaveChangesAsync(stoppingToken);

        foreach (var (proposal, citizen, content) in Comments)
        {
            if (proposal >= proposals.Count) continue;
            context.Comments.Add(new Comment
            {
                Content = content,
                ProposalId = proposals[proposal].Id,
                UserId = citizens[citizen].Id,
                CreatedAt = proposals[proposal].CreatedAt.AddHours(citizen + 3)
            });
        }

        await context.SaveChangesAsync(stoppingToken);
        logger.LogWarning("Demo data added: {Proposals} proposals, {Citizens} citizens", proposals.Count, citizens.Count);
    }
}
