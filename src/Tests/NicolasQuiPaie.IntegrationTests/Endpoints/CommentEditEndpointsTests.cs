namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>
/// A comment may be edited by its author, or by an administrator; nobody else.
/// </summary>
[TestFixture]
public class CommentEditEndpointsTests
{
    private const string Author = "comment-author";
    private const string Admin = "comment-admin";
    private const string Stranger = "comment-stranger";

    private NicolasQuiPaieApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new NicolasQuiPaieApiFactory();
        _client = _factory.CreateClient();
        (await _factory.InitializeTestDataAsync()).ShouldBeTrue();

        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var (id, name) in new[] { (Author, "Alice Autrice"), (Admin, "Ada Admin"), (Stranger, "Sam Passant") })
        {
            var user = new ApplicationUser { Id = id, UserName = $"{id}@test.com", Email = $"{id}@test.com", DisplayName = name };
            (await users.CreateAsync(user)).Succeeded.ShouldBeTrue();
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<int> AddCommentOfAuthorAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var comment = new Comment { Content = "Texte d'origine", UserId = Author, ProposalId = 1 };
        context.Comments.Add(comment);
        await context.SaveChangesAsync();
        return comment.Id;
    }

    private async Task<string> StoredContentAsync(int commentId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await context.Comments.FindAsync(commentId))!.Content;
    }

    private Task<HttpResponseMessage> EditAsync(int commentId, string content, string? userId, params string[] roles)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/comments/{commentId}")
        {
            Content = JsonContent.Create(new UpdateCommentDto { Content = content })
        };
        return _client.SendAsync(userId is null ? request : request.Authorized(userId, roles));
    }

    [Test]
    public async Task TheAuthor_WhoIsNotAdmin_EditsTheirComment()
    {
        var commentId = await AddCommentOfAuthorAsync();

        using var response = await EditAsync(commentId, "Corrigé par l'autrice", Author, NicolasRoles.User);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var edited = await response.Content.ReadFromJsonAsync<CommentDto>();
        edited!.Content.ShouldBe("Corrigé par l'autrice");
        edited.UserDisplayName.ShouldBe($"Display {Author}");
        edited.UpdatedAt.ShouldNotBeNull();
        (await StoredContentAsync(commentId)).ShouldBe("Corrigé par l'autrice");
    }

    [Test]
    public async Task AnAdmin_EditsSomeoneElsesComment_AndTheCommentKeepsItsAuthorsName()
    {
        var commentId = await AddCommentOfAuthorAsync();

        using var response = await EditAsync(commentId, "Modéré par l'administration", Admin, NicolasRoles.User, NicolasRoles.Admin);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var edited = await response.Content.ReadFromJsonAsync<CommentDto>();
        edited!.UserId.ShouldBe(Author);
        edited.UserDisplayName.ShouldBe("Alice Autrice");
        (await StoredContentAsync(commentId)).ShouldBe("Modéré par l'administration");
    }

    [TestCase(NicolasRoles.User)]
    [TestCase(NicolasRoles.SuperUser)]
    public async Task AnotherUser_WhoIsNotAdmin_IsForbidden(string role)
    {
        var commentId = await AddCommentOfAuthorAsync();

        using var response = await EditAsync(commentId, "Pas mon commentaire", Stranger, role);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await StoredContentAsync(commentId)).ShouldBe("Texte d'origine");
    }

    [Test]
    public async Task AnAnonymousCaller_IsUnauthorized()
    {
        var commentId = await AddCommentOfAuthorAsync();

        using var response = await EditAsync(commentId, "Anonyme", userId: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await StoredContentAsync(commentId)).ShouldBe("Texte d'origine");
    }

    [Test]
    public async Task AnUnknownComment_IsNotFound_EvenForAnAdmin()
    {
        using var response = await EditAsync(987654, "Rien", Admin, NicolasRoles.Admin);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
