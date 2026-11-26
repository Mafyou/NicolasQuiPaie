namespace NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>The categories created with the database (for signed-in users), and the health probe App Service calls.</summary>
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

    [Fact]
    public async Task GetCategories_ShouldReturnUnauthorized_WhenNotSignedIn()
    {
        // Arrange
        using var anonymousClient = factory.CreateClient();

        // Act
        var response = await anonymousClient.GetAsync("/api/categories", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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
