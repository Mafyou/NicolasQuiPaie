namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>
/// Categories are public reference data: the list of proposals filters on them before anyone signs in.
/// </summary>
[TestFixture]
public class CategoryEndpointsTests
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

    [Test]
    public async Task AnAnonymousVisitor_ReadsAllCategories()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");

        categories.ShouldNotBeNull();
        categories.Select(c => c.Name).ShouldBe(["Test Category 1", "Test Category 2"], ignoreOrder: true);
    }

    [Test]
    public async Task AnAnonymousVisitor_ReadsTheActiveCategories()
    {
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories/active");

        categories.ShouldNotBeNull();
        categories.Count.ShouldBe(2);
    }

    [Test]
    public async Task AnAnonymousVisitor_ReadsOneCategory()
    {
        var category = await _client.GetFromJsonAsync<CategoryDto>("/api/categories/1");

        category.ShouldNotBeNull();
        category.Name.ShouldBe("Test Category 1");
    }

    [Test]
    public async Task AnUnknownCategory_IsStillNotFound()
    {
        using var response = await _client.GetAsync("/api/categories/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task NoWriteIsOpenOnCategories()
    {
        using var post = await _client.PostAsJsonAsync("/api/categories", new { Name = "Nouvelle" });
        using var delete = await _client.DeleteAsync("/api/categories/1");

        post.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        delete.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
    }
}
