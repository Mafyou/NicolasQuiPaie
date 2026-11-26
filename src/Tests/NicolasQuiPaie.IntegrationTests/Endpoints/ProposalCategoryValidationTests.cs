namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>
/// A proposal filed under a category that does not exist is refused with a validation message, not a server error.
/// </summary>
[TestFixture]
public class ProposalCategoryValidationTests
{
    private NicolasQuiPaieApiFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new NicolasQuiPaieApiFactory();
        _client = _factory.CreateClient();
        (await _factory.InitializeTestDataAsync()).ShouldBeTrue();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private static CreateProposalDto Proposal(int categoryId) => new()
    {
        Title = $"Proposition dans la catégorie {categoryId}",
        Description = "Une proposition de test assez longue pour passer toutes les règles de validation du formulaire.",
        CategoryId = categoryId
    };

    private Task<HttpResponseMessage> CreateAsync(CreateProposalDto proposal)
        => _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/proposals") { Content = JsonContent.Create(proposal) }
            .Authorized("test-user-1", NicolasRoles.User));

    private int ProposalCount()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Proposals.Count();
    }

    [TestCase(999)]
    [TestCase(0)]
    [TestCase(-3)]
    public async Task AnUnknownCategory_GivesA400WithTheMessage_AndNothingIsSaved(int categoryId)
    {
        var before = ProposalCount();

        using var response = await CreateAsync(Proposal(categoryId));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        problem!.Errors[nameof(CreateProposalDto.CategoryId)].ShouldBe([ProposalMessages.UnknownCategory]);
        ProposalCount().ShouldBe(before);
    }

    [Test]
    public async Task AnExistingCategory_StillCreatesTheProposal()
    {
        using var response = await CreateAsync(Proposal(2));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ProposalDto>();
        created!.CategoryId.ShouldBe(2);
        created.CreatedById.ShouldBe("test-user-1");
    }
}
