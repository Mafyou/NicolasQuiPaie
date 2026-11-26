namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>A proposal from its creation to its deletion: only its author may change it, and the views are counted.</summary>
public class ProposalLifecycleTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private static CreateProposalDto NewProposal(string title) => new()
    {
        Title = title,
        Description = "Une proposition écrite pour le test du cycle de vie, assez longue pour passer la validation.",
        CategoryId = 3,
        Tags = "cycle,test"
    };

    [Fact]
    public async Task CreatedProposal_ShouldBeReadable_AndListedInItsCategory()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();

        // Act
        var created = await CreateAsync(author, "Une proposition à lire");

        // Assert
        var read = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{created.Id}", Cancel);
        read.ShouldNotBeNull().Title.ShouldBe("Une proposition à lire");
        read.CategoryId.ShouldBe(3);
        read.CreatedById.ShouldBe(NicolasQuiPaieApiFactory.TestUserId);
        var inCategory = await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals?categoryId=3", Cancel);
        inCategory.ShouldNotBeNull().ShouldContain(proposal => proposal.Id == created.Id);
    }

    [Fact]
    public async Task UpdateProposal_ShouldChangeIt_ForItsAuthor()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();
        var created = await CreateAsync(author, "Avant la modification");
        var update = new UpdateProposalDto
        {
            Title = "Après la modification",
            Description = "La description a changé, elle aussi, et elle reste assez longue pour la validation.",
            CategoryId = 4
        };

        // Act
        var response = await author.PutAsJsonAsync($"/api/proposals/{created.Id}", update, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var read = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{created.Id}", Cancel);
        read.ShouldNotBeNull().Title.ShouldBe(update.Title);
        read.CategoryId.ShouldBe(4);
        read.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task UpdateProposal_ShouldReturnForbidden_ForSomeoneElse()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();
        using var stranger = await factory.CreateAuthenticatedClientAsync(NicolasQuiPaieApiFactory.OtherUserId);
        var created = await CreateAsync(author, "Pas touche à ma proposition");

        // Act
        var response = await stranger.PutAsJsonAsync($"/api/proposals/{created.Id}",
            new UpdateProposalDto { Title = "Détournée", Description = "Une description qui ne devrait jamais être enregistrée par l'API.", CategoryId = 1 }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{created.Id}", Cancel)).ShouldNotBeNull().Title.ShouldBe("Pas touche à ma proposition");
    }

    [Fact]
    public async Task UpdateProposal_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();

        // Act
        var response = await author.PutAsJsonAsync("/api/proposals/99999",
            new UpdateProposalDto { Title = "Fantôme", Description = "Une description pour une proposition qui n'existe pas du tout.", CategoryId = 1 }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProposal_ShouldRemoveIt_WithItsVotes_ForItsAuthor()
    {
        // Arrange: a proposal with a vote of someone else on it
        using var author = await factory.CreateAuthenticatedClientAsync();
        using var voter = await factory.CreateAuthenticatedClientAsync(NicolasQuiPaieApiFactory.OtherUserId);
        var created = await CreateAsync(author, "Une proposition à supprimer");
        (await voter.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = created.Id, VoteType = DtoVoteType.Against }, Cancel))
            .StatusCode.ShouldBe(HttpStatusCode.Created);

        // Act
        var response = await author.DeleteAsync($"/api/proposals/{created.Id}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/proposals/{created.Id}", Cancel)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await factory.WithContextAsync(context => context.Votes.CountAsync(vote => vote.ProposalId == created.Id, Cancel))).ShouldBe(0);
    }

    [Fact]
    public async Task DeleteProposal_ShouldReturnForbidden_ForSomeoneElse()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();
        using var stranger = await factory.CreateAuthenticatedClientAsync(NicolasQuiPaieApiFactory.OtherUserId);
        var created = await CreateAsync(author, "Une proposition à garder");

        // Act
        var response = await stranger.DeleteAsync($"/api/proposals/{created.Id}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await _client.GetAsync($"/api/proposals/{created.Id}", Cancel)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RecordView_ShouldIncrementTheViewCounter()
    {
        // Arrange
        var before = (await _client.GetFromJsonAsync<ProposalDto>("/api/proposals/6", Cancel)).ShouldNotBeNull().ViewsCount;

        // Act
        var response = await _client.PostAsync("/api/proposals/6/views", content: null, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stored = await factory.WithContextAsync(context => context.Proposals.SingleAsync(proposal => proposal.Id == 6, Cancel));
        stored.ViewsCount.ShouldBe(before + 1);
    }

    [Fact]
    public async Task Votes_ShouldKeepTheProposalCountersInStep()
    {
        // Arrange
        using var author = await factory.CreateAuthenticatedClientAsync();
        using var other = await factory.CreateAuthenticatedClientAsync(NicolasQuiPaieApiFactory.OtherUserId);
        var created = await CreateAsync(author, "Une proposition très disputée");

        // Act: one for, one against, then the first voter changes their mind
        await author.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = created.Id, VoteType = DtoVoteType.For }, Cancel);
        await other.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = created.Id, VoteType = DtoVoteType.Against }, Cancel);
        await author.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = created.Id, VoteType = DtoVoteType.Against }, Cancel);

        // Assert
        var read = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{created.Id}", Cancel);
        read.ShouldNotBeNull().VotesFor.ShouldBe(0);
        read.VotesAgainst.ShouldBe(2);
        var votes = await _client.GetFromJsonAsync<List<VoteDto>>($"/api/votes/proposal/{created.Id}", Cancel);
        votes.ShouldNotBeNull().Count.ShouldBe(2);
    }

    private static async Task<ProposalDto> CreateAsync(HttpClient author, string title)
    {
        var response = await author.PostAsJsonAsync("/api/proposals", NewProposal(title), Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ProposalDto>(Cancel)).ShouldNotBeNull();
    }
}
