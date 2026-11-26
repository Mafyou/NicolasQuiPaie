namespace NicolasQuiPaie.IntegrationTests.SqlServer;

/// <summary>
/// A SQL Server of its own in a container (Testcontainers), for what neither SQLite nor the in-memory store can tell:
/// the real migrations, the "nqp" schema and the API's own start-up seeding. Docker is required: without it (the usual
/// case on a development machine) the tests are skipped; GitHub's Ubuntu runners have it.
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;

    public string? SkipReason { get; private set; }

    public string ConnectionString => _container?.GetConnectionString() ?? throw new InvalidOperationException(SkipReason);

    public async ValueTask InitializeAsync()
    {
        try
        {
            _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _container.StartAsync(TestContext.Current.CancellationToken);
        }
        catch (Exception exception) when (exception is DockerUnavailableException or TimeoutException or HttpRequestException)
        {
            SkipReason = $"Docker is not available here: {exception.Message}";
            _container = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

/// <summary>The API started as in production, migrations and seeding included, on the container's database.</summary>
public sealed class SqlServerApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.UseSetting("DisableHttpsRedirection", "true");
    }
}

public class SqlServerApiTests(SqlServerFixture sqlServer) : IClassFixture<SqlServerFixture>
{
    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    [Fact]
    public async Task StartUp_ShouldMigrateTheDatabase_AndSeedTheCategoriesAndTheTestUser()
    {
        Assert.SkipWhen(sqlServer.SkipReason is not null, sqlServer.SkipReason ?? "");

        // Arrange
        await using var factory = new SqlServerApiFactory(sqlServer.ConnectionString);
        using var client = factory.CreateClient();

        // Act
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = "nicolas@test.fr", Password = "Test123!" }, Cancel);

        // Assert
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await context.Database.GetPendingMigrationsAsync(Cancel)).ShouldBeEmpty();
        (await context.Categories.CountAsync(Cancel)).ShouldBeGreaterThan(0);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Journey_ShouldRegisterProposeVoteAndComment_OnSqlServer()
    {
        Assert.SkipWhen(sqlServer.SkipReason is not null, sqlServer.SkipReason ?? "");

        // Arrange
        await using var factory = new SqlServerApiFactory(sqlServer.ConnectionString);
        using var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = $"sql-{Guid.NewGuid():N}@test.com", Password = "Nicolas123", DisplayName = "Nicolas SQL" }, Cancel);
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        // Act
        var created = await client.PostAsJsonAsync("/api/proposals", new CreateProposalDto
        {
            Title = "Une proposition sur SQL Server",
            Description = "Une proposition enregistrée dans un vrai SQL Server, migrations comprises, pour le test.",
            CategoryId = 1
        }, Cancel);
        var proposal = (await created.Content.ReadFromJsonAsync<ProposalDto>(Cancel)).ShouldNotBeNull();
        var vote = await client.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = proposal.Id, VoteType = DtoVoteType.For }, Cancel);
        var comment = await client.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = proposal.Id, Content = "Validé sur SQL Server" }, Cancel);
        var unknownProposalVote = await client.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = 987654, VoteType = DtoVoteType.For }, Cancel);

        // Assert
        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        vote.StatusCode.ShouldBe(HttpStatusCode.Created);
        comment.StatusCode.ShouldBe(HttpStatusCode.Created);
        unknownProposalVote.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var read = await client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposal.Id}", Cancel);
        read.ShouldNotBeNull().VotesFor.ShouldBe(1);
    }
}
