namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>The categories created with the database (public reference data, read by anyone), and the health probe App Service calls.</summary>
public class CategoryAndHealthEndpointsTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await factory.CreateTokenAsync());

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData("/api/categories")]
    [InlineData("/api/categories/active")]
    [InlineData("/api/categories/2")]
    public async Task GetCategories_ShouldAnswer_WhenNotSignedIn(string path)
    {
        // Arrange
        using var anonymousClient = factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync(path, Cancel);

        // Assert: the proposals page filters on them before anyone signs in
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCategories_ShouldListTheSameCategories_SignedInOrNot()
    {
        // Arrange
        using var anonymousClient = factory.CreateClient();

        // Act
        var anonymous = await anonymousClient.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel);
        var signedIn = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel);

        // Assert
        anonymous.ShouldNotBeNull().Select(category => category.Id).ShouldBe(signedIn.ShouldNotBeNull().Select(category => category.Id));
    }

    [Fact]
    public async Task Categories_ShouldOfferNoWrite_EvenToAnAnonymousCaller()
    {
        // Arrange
        using var anonymousClient = factory.CreateClient();

        // Act
        var post = await anonymousClient.PostAsJsonAsync("/api/categories", new { Name = "Nouvelle" }, Cancel);
        var put = await anonymousClient.PutAsJsonAsync("/api/categories/1", new { Name = "Renommée" }, Cancel);
        var delete = await anonymousClient.DeleteAsync("/api/categories/1", Cancel);

        // Assert: reads only; the categories stay as the database created them
        post.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        put.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        delete.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        (await factory.WithContextAsync(context => context.Categories.AnyAsync(category => category.Name == "Nouvelle" || category.Name == "Renommée", Cancel)))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task GetCategories_ShouldReturnTheSeededCategories_InTheirOrder()
    {
        // Arrange
        var stored = await factory.WithContextAsync(context => context.Categories.CountAsync(Cancel));

        // Act
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories", Cancel);

        // Assert
        stored.ShouldBeGreaterThan(0);
        categories.ShouldNotBeNull().Count.ShouldBe(stored);
        categories.Select(category => category.SortOrder).ShouldBeInOrder();
        categories[0].Name.ShouldBe("Fiscalité");
    }

    [Fact]
    public async Task GetActiveCategories_ShouldOnlyReturnActiveOnes()
    {
        // Act
        var categories = await _client.GetFromJsonAsync<List<CategoryDto>>("/api/categories/active", Cancel);

        // Assert
        categories.ShouldNotBeNull().ShouldNotBeEmpty();
        categories.ShouldAllBe(category => category.IsActive);
    }

    [Fact]
    public async Task GetCategory_ShouldReturnIt_OrNotFound()
    {
        // Act
        var found = await _client.GetAsync("/api/categories/2", Cancel);
        var missing = await _client.GetAsync("/api/categories/999", Cancel);

        // Assert
        found.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await found.Content.ReadFromJsonAsync<CategoryDto>(Cancel)).ShouldNotBeNull().Name.ShouldBe("Dépenses Publiques");
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_ShouldAnswerHealthy()
    {
        // Act
        using var anonymousClient = factory.CreateClient();
        var response = await anonymousClient.GetAsync("/health", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var health = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancel));
        health.RootElement.GetProperty("status").GetString().ShouldBe("Healthy");
    }
}
