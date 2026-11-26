namespace NicolasQuiPaie.FunctionalTests.Api;

/// <summary>
/// The API as its clients use it, black box: plain HTTP to its address, every user signed up through its own
/// registration, the demo site's proposals already there.
/// </summary>
public class CitizenJourneyTests(Platform platform) : IDisposable
{
    private readonly HttpClient _client = platform.CreateApiClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Visitor_ShouldBrowseTheProposals_ButNotVoteNorSeeTheAnalytics()
    {
        // Act
        var proposals = await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals?take=100", Cancel);
        var trending = await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals/trending", Cancel);
        var first = proposals.ShouldNotBeNull().First();
        var detail = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{first.Id}", Cancel);
        var vote = await _client.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = first.Id, VoteType = VoteType.For }, Cancel);
        var analytics = await _client.GetAsync("/api/analytics/dashboard-stats", Cancel);

        // Assert: the demo site's proposals, all active, readable by anyone; acting needs an account
        proposals.Count.ShouldBeGreaterThan(5);
        proposals.ShouldAllBe(proposal => proposal.Status == ProposalStatus.Active);
        trending.ShouldNotBeNull().ShouldNotBeEmpty();
        detail.ShouldNotBeNull().Title.ShouldBe(first.Title);
        vote.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        analytics.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NewCitizen_ShouldSignUp_Propose_Vote_ChangeTheirMind_AndDebate()
    {
        // Arrange: signing up
        var citizen = await SignUpAsync("Nicolas Contribuable");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", citizen.Token);
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel);
        var category = categories.ShouldNotBeNull().First();

        // Act: a proposal, a vote for it, a change of mind, a comment
        var created = await _client.PostAsJsonAsync("/api/proposals", new CreateProposalDto
        {
            Title = $"Supprimer la taxe sur les parcours {Guid.NewGuid():N}"[..60],
            Description = "Une proposition écrite pendant le parcours fonctionnel, assez longue pour passer les règles.",
            CategoryId = category.Id,
            Tags = "parcours,test"
        }, Cancel);
        var proposal = (await created.Content.ReadFromJsonAsync<ProposalDto>(Cancel)).ShouldNotBeNull();
        var voteFor = await _client.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = proposal.Id, VoteType = VoteType.For }, Cancel);
        var afterFor = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposal.Id}", Cancel);
        var voteAgainst = await _client.PostAsJsonAsync("/api/votes",
            new CreateVoteDto { ProposalId = proposal.Id, VoteType = VoteType.Against, Comment = "Tout bien réfléchi, non." }, Cancel);
        var afterAgainst = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposal.Id}", Cancel);
        var myVote = await _client.GetFromJsonAsync<VoteDto>($"/api/votes/user/{citizen.User.Id}/proposal/{proposal.Id}", Cancel);
        var comment = await _client.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = proposal.Id, Content = "Qui paiera, sinon ?" }, Cancel);
        var comments = await _client.GetFromJsonAsync<List<CommentDto>>($"/api/comments/proposal/{proposal.Id}", Cancel);

        // Assert
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        proposal.CreatedById.ShouldBe(citizen.User.Id);
        voteFor.StatusCode.ShouldBe(HttpStatusCode.Created);
        afterFor.ShouldNotBeNull().VotesFor.ShouldBe(1);
        voteAgainst.IsSuccessStatusCode.ShouldBeTrue();
        afterAgainst.ShouldNotBeNull().VotesFor.ShouldBe(0);
        afterAgainst.VotesAgainst.ShouldBe(1);
        myVote.ShouldNotBeNull().VoteType.ShouldBe(VoteType.Against);
        myVote.Comment.ShouldBe("Tout bien réfléchi, non.");
        comment.StatusCode.ShouldBe(HttpStatusCode.Created);
        comments.ShouldNotBeNull().ShouldContain(item => item.Content == "Qui paiera, sinon ?" && item.UserDisplayName == "Nicolas Contribuable");
    }

    [Fact]
    public async Task ReturningCitizen_ShouldSignInAgain_AndFindTheirVote_ThenWithdrawIt()
    {
        // Arrange: an account that voted once
        const string password = "Retour2026";
        var citizen = await SignUpAsync("Nicolas de Retour", password);
        var proposalId = (await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals", Cancel)).ShouldNotBeNull().First().Id;
        using (var firstVisit = platform.CreateApiClient())
        {
            firstVisit.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", citizen.Token);
            (await firstVisit.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = proposalId, VoteType = VoteType.For }, Cancel))
                .StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        // Act: a new visit, signed in with the password
        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = citizen.User.Email!, Password = password }, Cancel);
        var session = (await login.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        var myVotes = await _client.GetFromJsonAsync<List<VoteDto>>($"/api/votes/user/{citizen.User.Id}", Cancel);
        var before = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposalId}", Cancel);
        var withdraw = await _client.DeleteAsync($"/api/votes/user/{citizen.User.Id}/proposal/{proposalId}", Cancel);
        var after = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposalId}", Cancel);

        // Assert
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        session.User.Id.ShouldBe(citizen.User.Id);
        myVotes.ShouldNotBeNull().ShouldContain(vote => vote.ProposalId == proposalId && vote.VoteType == VoteType.For);
        withdraw.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        after.ShouldNotBeNull().VotesFor.ShouldBe(before.ShouldNotBeNull().VotesFor - 1);
        (await _client.GetAsync($"/api/votes/user/{citizen.User.Id}/proposal/{proposalId}", Cancel)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Author_ShouldEditAndWithdrawTheirProposal_WhileAnotherCitizenCannot()
    {
        // Arrange: two citizens, a proposal of the first
        var author = await SignUpAsync("Nicolas Auteur");
        var other = await SignUpAsync("Nicolas Voisin");
        using var authorClient = AuthenticatedClient(author.Token);
        using var otherClient = AuthenticatedClient(other.Token);
        var created = await authorClient.PostAsJsonAsync("/api/proposals", new CreateProposalDto
        {
            Title = "Une proposition à corriger plus tard",
            Description = "La première version de la proposition, qui sera corrigée puis retirée par son auteur.",
            CategoryId = 1
        }, Cancel);
        var proposal = (await created.Content.ReadFromJsonAsync<ProposalDto>(Cancel)).ShouldNotBeNull();
        var update = new UpdateProposalDto
        {
            Title = "Une proposition corrigée",
            Description = "La seconde version de la proposition, corrigée par son auteur avant d'être retirée.",
            CategoryId = 2
        };

        // Act
        var hijack = await otherClient.PutAsJsonAsync($"/api/proposals/{proposal.Id}", update, Cancel);
        var edit = await authorClient.PutAsJsonAsync($"/api/proposals/{proposal.Id}", update, Cancel);
        var edited = await _client.GetFromJsonAsync<ProposalDto>($"/api/proposals/{proposal.Id}", Cancel);
        var otherDelete = await otherClient.DeleteAsync($"/api/proposals/{proposal.Id}", Cancel);
        var authorDelete = await authorClient.DeleteAsync($"/api/proposals/{proposal.Id}", Cancel);

        // Assert
        hijack.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        edit.StatusCode.ShouldBe(HttpStatusCode.OK);
        edited.ShouldNotBeNull().Title.ShouldBe(update.Title);
        otherDelete.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        authorDelete.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/proposals/{proposal.Id}", Cancel)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Visitor_ShouldReadTheCategories_WithoutAnAccount()
    {
        // Act: the proposals page filters on them before anyone signs in
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel);
        var active = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories/active", Cancel);
        var create = await _client.PostAsJsonAsync("/api/categories", new { Name = "Catégorie d'un visiteur" }, Cancel);

        // Assert: readable by anyone, never writable
        categories.ShouldNotBeNull().ShouldNotBeEmpty();
        active.ShouldNotBeNull().ShouldNotBeEmpty();
        create.IsSuccessStatusCode.ShouldBeFalse();
        (await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel)).ShouldNotBeNull().Count.ShouldBe(categories.Count);
    }

    [Fact]
    public async Task Citizen_ShouldCorrectTheirOwnComment_WhileAnotherCitizenCannot()
    {
        // Arrange: two citizens without any role but User, a comment of the first
        var author = await SignUpAsync("Nicolas Commentateur");
        var other = await SignUpAsync("Nicolas Correcteur");
        using var authorClient = AuthenticatedClient(author.Token);
        using var otherClient = AuthenticatedClient(other.Token);
        var proposalId = (await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals", Cancel)).ShouldNotBeNull().First().Id;
        var posted = await authorClient.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = proposalId, Content = "Avec une fote" }, Cancel);
        var comment = (await posted.Content.ReadFromJsonAsync<CommentDto>(Cancel)).ShouldNotBeNull();

        // Act
        var hijack = await otherClient.PutAsJsonAsync($"/api/comments/{comment.Id}", new UpdateCommentDto { Content = "Réécrit par un autre" }, Cancel);
        var correction = await authorClient.PutAsJsonAsync($"/api/comments/{comment.Id}", new UpdateCommentDto { Content = "Sans la faute" }, Cancel);
        var comments = await _client.GetFromJsonAsync<List<CommentDto>>($"/api/comments/proposal/{proposalId}", Cancel);

        // Assert
        hijack.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        correction.StatusCode.ShouldBe(HttpStatusCode.OK);
        comments.ShouldNotBeNull().ShouldContain(item => item.Id == comment.Id && item.Content == "Sans la faute" && item.UserDisplayName == "Nicolas Commentateur");
    }

    [Fact]
    public async Task Citizen_ShouldBeToldTheCategoryDoesNotExist_AndNothingIsFiled()
    {
        // Arrange
        var citizen = await SignUpAsync("Nicolas Égaré");
        using var client = AuthenticatedClient(citizen.Token);
        var title = $"Dans une catégorie qui n'existe pas {Guid.NewGuid():N}"[..60];

        // Act
        var response = await client.PostAsJsonAsync("/api/proposals", new CreateProposalDto
        {
            Title = title,
            Description = "Une proposition rangée sous une catégorie que la base ne connaît pas, refusée proprement.",
            CategoryId = 98765
        }, Cancel);

        // Assert: a validation problem on the category, not a 500
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancel));
        problem.RootElement.GetProperty("errors").GetProperty(nameof(CreateProposalDto.CategoryId))[0].GetString()
            .ShouldBe("Cette catégorie n'existe pas : veuillez en choisir une dans la liste");
        (await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals?take=100", Cancel)).ShouldNotBeNull()
            .ShouldNotContain(proposal => proposal.Title == title);
    }

    [Fact]
    public async Task SignedInCitizen_ShouldSeeTheirVoteInTheDashboard()
    {
        // Arrange
        var citizen = await SignUpAsync("Nicolas Statisticien");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", citizen.Token);
        var before = (await _client.GetFromJsonAsync<GlobalStatsDto>("/api/analytics/global-stats", Cancel)).ShouldNotBeNull();
        var proposalId = (await _client.GetFromJsonAsync<List<ProposalDto>>("/api/proposals", Cancel)).ShouldNotBeNull().Last().Id;

        // Act
        await _client.PostAsJsonAsync("/api/votes", new CreateVoteDto { ProposalId = proposalId, VoteType = VoteType.Against }, Cancel);
        var after = (await _client.GetFromJsonAsync<GlobalStatsDto>("/api/analytics/global-stats", Cancel)).ShouldNotBeNull();
        var dashboard = (await _client.GetFromJsonAsync<DashboardStatsDto>("/api/analytics/dashboard-stats", Cancel)).ShouldNotBeNull();

        // Assert
        after.TotalVotes.ShouldBe(before.TotalVotes + 1);
        after.TotalUsers.ShouldBeGreaterThanOrEqualTo(before.TotalUsers);
        dashboard.RasLebolMeter.ShouldBeInRange(0, 100);
    }

    [Fact]
    public async Task SignUp_ShouldRefuseTheSameEmailTwice_AndAWrongPasswordAfterwards()
    {
        // Arrange
        var citizen = await SignUpAsync("Nicolas Unique");

        // Act
        var again = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = citizen.User.Email!, Password = "Nicolas123", DisplayName = "Copie" }, Cancel);
        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = citizen.User.Email!, Password = "PasLeBon123" }, Cancel);

        // Assert
        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private HttpClient AuthenticatedClient(string token)
    {
        var client = platform.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private async Task<AuthResponseDto> SignUpAsync(string displayName, string password = "Nicolas123")
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = $"citizen-{Guid.NewGuid():N}@test.com", Password = password, DisplayName = displayName }, Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull();
        auth.Success.ShouldBeTrue();
        return auth;
    }
}
