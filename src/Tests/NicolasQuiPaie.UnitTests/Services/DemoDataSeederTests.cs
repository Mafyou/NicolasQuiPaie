namespace NicolasQuiPaie.UnitTests.Services;

/// <summary>The demo data: only when SeedDemoData is on, only on an empty platform, with real votes behind the counters.</summary>
[TestFixture]
public class DemoDataSeederTests
{
    private ServiceProvider _services = null!;

    [SetUp]
    public async Task SetUp()
    {
        var name = $"demo-{Guid.NewGuid():N}";
        _services = new ServiceCollection()
            .AddLogging()
            .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(name))
            .AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .Services
            .BuildServiceProvider();
        // The categories come from HasData, created with the database.
        await Context.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task TearDown() => await _services.DisposeAsync();

    private ApplicationDbContext Context => _services.GetRequiredService<ApplicationDbContext>();

    private static IConfiguration Settings(bool seed) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { [DemoDataSeeder.SettingKey] = $"{seed}" }).Build();

    private Task SeedAsync(bool seed) =>
        DemoDataSeeder.SeedAsync(Settings(seed), Context, _services.GetRequiredService<UserManager<ApplicationUser>>(), NullLogger.Instance);

    [Test]
    public async Task SeedAsync_WhenOff_ShouldAddNothing()
    {
        await SeedAsync(seed: false);

        (await Context.Proposals.AnyAsync()).ShouldBeFalse();
    }

    [Test]
    public async Task SeedAsync_OnAnEmptyPlatform_ShouldAddActiveProposalsWhoseCountersMatchTheirVotes()
    {
        await SeedAsync(seed: true);

        var proposals = await Context.Proposals.Include(proposal => proposal.Votes).ToListAsync();
        proposals.Count.ShouldBeGreaterThan(5);
        proposals.ShouldAllBe(proposal => proposal.Status == InfrastructureProposalStatus.Active);
        foreach (var proposal in proposals)
        {
            proposal.VotesFor.ShouldBe(proposal.Votes.Count(vote => vote.VoteType == InfrastructureVoteType.For));
            proposal.VotesAgainst.ShouldBe(proposal.Votes.Count(vote => vote.VoteType == InfrastructureVoteType.Against));
        }
        (await Context.Comments.AnyAsync()).ShouldBeTrue();
    }

    [Test]
    public async Task SeedAsync_WhenProposalsExist_ShouldNotAddMore()
    {
        await SeedAsync(seed: true);
        var count = await Context.Proposals.CountAsync();

        await SeedAsync(seed: true);

        (await Context.Proposals.CountAsync()).ShouldBe(count);
    }
}
