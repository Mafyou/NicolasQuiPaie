namespace NicolasQuiPaie.UnitTests.Services;

/// <summary>The demo data: only when SeedDemoData is on, only on an empty platform, with real votes behind the counters.</summary>
public class DemoDataSeederTests : IAsyncLifetime
{
    private ServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
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

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private ApplicationDbContext Context => _services.GetRequiredService<ApplicationDbContext>();

    private static IConfiguration Settings(bool seed) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { [DemoDataSeeder.SettingKey] = $"{seed}" }).Build();

    private Task SeedAsync(bool seed) =>
        DemoDataSeeder.SeedAsync(Settings(seed), Context, _services.GetRequiredService<UserManager<ApplicationUser>>(), NullLogger.Instance);

    [Fact]
    public async Task SeedAsync_WhenOff_ShouldAddNothing()
    {
        await SeedAsync(seed: false);

        (await Context.Proposals.AnyAsync(TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task SeedAsync_OnAnEmptyPlatform_ShouldAddActiveProposalsWhoseCountersMatchTheirVotes()
    {
        await SeedAsync(seed: true);

        var proposals = await Context.Proposals.Include(proposal => proposal.Votes).ToListAsync(TestContext.Current.CancellationToken);
        proposals.Count.ShouldBeGreaterThan(5);
        proposals.ShouldAllBe(proposal => proposal.Status == InfrastructureProposalStatus.Active);
        foreach (var proposal in proposals)
        {
            proposal.VotesFor.ShouldBe(proposal.Votes.Count(vote => vote.VoteType == InfrastructureVoteType.For));
            proposal.VotesAgainst.ShouldBe(proposal.Votes.Count(vote => vote.VoteType == InfrastructureVoteType.Against));
        }
        (await Context.Comments.AnyAsync(TestContext.Current.CancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsync_WhenProposalsExist_ShouldNotAddMore()
    {
        await SeedAsync(seed: true);
        var count = await Context.Proposals.CountAsync(TestContext.Current.CancellationToken);

        await SeedAsync(seed: true);

        (await Context.Proposals.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(count);
    }
}
