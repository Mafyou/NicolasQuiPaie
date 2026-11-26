namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>The debate under a proposal: comments written, listed and deleted through the real repositories.</summary>
public class CommentEndpointsTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task AuthenticateAsync(string userId = NicolasQuiPaieApiFactory.TestUserId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await factory.CreateTokenAsync(userId));

    [Fact]
    public async Task PostComment_ShouldAddItUnderTheProposal()
    {
        // Arrange
        await AuthenticateAsync();
        var comment = new CreateCommentDto { ProposalId = 2, Content = "Et qui paie la note, encore une fois ?" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", comment, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<CommentDto>(Cancel)).ShouldNotBeNull();
        created.Id.ShouldBeGreaterThan(0);
        created.UserId.ShouldBe(NicolasQuiPaieApiFactory.TestUserId);
        created.UserDisplayName.ShouldBe("Test User");
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"/api/comments/{created.Id}");

        var listed = await _client.GetFromJsonAsync<List<CommentDto>>("/api/comments/proposal/2", Cancel);
        listed.ShouldNotBeNull().ShouldContain(item => item.Id == created.Id && item.Content == comment.Content);
    }

    [Fact]
    public async Task PostComment_ShouldAcceptAReply()
    {
        // Arrange
        await AuthenticateAsync();
        var parent = await PostAsync(new CreateCommentDto { ProposalId = 3, Content = "Premier avis" });

        // Act
        var reply = await PostAsync(new CreateCommentDto { ProposalId = 3, Content = "Je ne suis pas d'accord", ParentCommentId = parent.Id }, NicolasQuiPaieApiFactory.OtherUserId);

        // Assert
        reply.ParentCommentId.ShouldBe(parent.Id);
        var listed = await _client.GetFromJsonAsync<List<CommentDto>>("/api/comments/proposal/3", Cancel);
        listed.ShouldNotBeNull().Select(item => item.Id).ShouldContain(parent.Id);
    }

    [Fact]
    public async Task PostComment_ShouldReturnUnauthorized_WhenNotSignedIn()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = 1, Content = "Anonyme" }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(1, "")]
    [InlineData(1, "   ")]
    [InlineData(0, "Sur aucune proposition")]
    [InlineData(999, "Sur une proposition qui n'existe pas")]
    public async Task PostComment_ShouldReturnBadRequest_ForAnInvalidComment(int proposalId, string content)
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = proposalId, Content = content }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostComment_ShouldReturnBadRequest_WhenTooLong()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/comments", new CreateCommentDto { ProposalId = 1, Content = new string('A', 1001) }, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteComment_ShouldRemoveItFromTheList_ForItsAuthor()
    {
        // Arrange
        await AuthenticateAsync();
        var comment = await PostAsync(new CreateCommentDto { ProposalId = 4, Content = "À retirer" });

        // Act
        var response = await _client.DeleteAsync($"/api/comments/{comment.Id}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var listed = await _client.GetFromJsonAsync<List<CommentDto>>("/api/comments/proposal/4", Cancel);
        listed.ShouldNotBeNull().ShouldNotContain(item => item.Id == comment.Id);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnForbidden_ForSomeoneElse()
    {
        // Arrange: written by the other user, deleted by the test user
        var comment = await PostAsync(new CreateCommentDto { ProposalId = 5, Content = "Le mien" }, NicolasQuiPaieApiFactory.OtherUserId);
        await AuthenticateAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/comments/{comment.Id}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteComment_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.DeleteAsync("/api/comments/99999", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetComments_ShouldReturnAnEmptyList_ForAProposalWithoutDebate()
    {
        // Act
        var comments = await _client.GetFromJsonAsync<List<CommentDto>>("/api/comments/proposal/7", Cancel);

        // Assert
        comments.ShouldNotBeNull().ShouldBeEmpty();
    }

    /// <summary>Posts a comment as the given user, with a client of its own.</summary>
    private async Task<CommentDto> PostAsync(CreateCommentDto comment, string userId = NicolasQuiPaieApiFactory.TestUserId)
    {
        using var client = await factory.CreateAuthenticatedClientAsync(userId);
        var response = await client.PostAsJsonAsync("/api/comments", comment, Cancel);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CommentDto>(Cancel)).ShouldNotBeNull();
    }
}
