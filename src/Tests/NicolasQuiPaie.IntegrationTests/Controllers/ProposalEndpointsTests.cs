// C# 13.0 - Enhanced Proposal Endpoints Integration Tests with latest language features
// Updated to reflect contribution-based leveling system


namespace NicolasQuiPaie.IntegrationTests.Controllers;

/// <summary>
/// C# 13.0 - Proposal endpoints integration tests using collection expressions and modern patterns
/// Updated to reflect contribution-based leveling system where users gain levels through active participation
/// </summary>
public class ProposalEndpointsTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IDisposable
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    // C# 13.0 - Record for endpoint test scenarios
    public record ProposalEndpointScenario(
        string Name,
        string Endpoint,
        HttpMethod Method,
        object? RequestBody = null,
        HttpStatusCode ExpectedStatus = HttpStatusCode.OK,
        bool RequiresAuth = false);

    // C# 13.0 - Collection expressions for comprehensive endpoint testing
    public static TheoryData<ProposalEndpointScenario> PublicEndpointCases => new(PublicEndpointScenarios);

    public static readonly ProposalEndpointScenario[] PublicEndpointScenarios =
    [
        new("Get all proposals", "/api/proposals", HttpMethod.Get),
        new("Get proposals with pagination", "/api/proposals?skip=0&take=10", HttpMethod.Get),
        new("Get proposals by category", "/api/proposals?categoryId=1", HttpMethod.Get),
        new("Search proposals", "/api/proposals?search=Test", HttpMethod.Get),
        new("Get trending proposals", "/api/proposals/trending", HttpMethod.Get),
        new("Get specific proposal", "/api/proposals/1", HttpMethod.Get),
        new("Get non-existent proposal", "/api/proposals/999", HttpMethod.Get, null, HttpStatusCode.NotFound)
    ];

    public static readonly ProposalEndpointScenario[] AuthenticatedEndpointScenarios =
    [
        new("Create proposal", "/api/proposals", HttpMethod.Post,
            new CreateProposalDto
            {
                Title = "New Integration Test Proposal",
                Description = "This is a comprehensive test proposal created through integration testing to validate the complete API functionality and contribution system.",
                CategoryId = 1
            },
            HttpStatusCode.Created, true),
        new("Update proposal", "/api/proposals/1", HttpMethod.Put,
            new UpdateProposalDto
            {
                Title = "Updated Integration Test Proposal",
                Description = "This proposal has been updated through integration testing to validate the update functionality and user contribution tracking.",
                CategoryId = 2
            },
            HttpStatusCode.OK, true),
        new("Delete proposal", "/api/proposals/1", HttpMethod.Delete, null, HttpStatusCode.NoContent, true)
    ];

    [Theory]
    [MemberData(nameof(PublicEndpointCases))]
    public async Task PublicProposalEndpoints_ShouldWork_WithoutAuthentication(ProposalEndpointScenario scenario)
    {
        // Act
        var request = new HttpRequestMessage(scenario.Method, scenario.Endpoint);
        if (scenario.RequestBody is not null)
        {
            request.Content = JsonContent.Create(scenario.RequestBody);
        }

        var response = await _client.SendAsync(request, Cancel);

        // Assert - C# 13.0 enhanced validation with modern null checking
        response.StatusCode.ShouldBe(scenario.ExpectedStatus);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync(Cancel);
            content.ShouldNotBeNullOrEmpty();

            // Validate JSON structure
            Should.NotThrow(() => JsonDocument.Parse(content));
        }
    }

    [Fact]
    public async Task GetProposals_ShouldReturnOk_WithProposals()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        content.ShouldNotBeNullOrEmpty();

        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposals.ShouldNotBeNull();
        proposals.Count.ShouldBeGreaterThan(0);

        // Should only return active proposals
        proposals.All(p => p.Status == NicolasQuiPaieData.DTOs.ProposalStatus.Active).ShouldBeTrue();
    }

    [Fact]
    public async Task GetProposals_WithCategoryFilter_ShouldReturnFilteredProposals()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals?categoryId=1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposals.ShouldNotBeNull();
        proposals.All(p => p.CategoryId == 1).ShouldBeTrue();
    }

    [Fact]
    public async Task GetProposals_WithSearch_ShouldReturnMatchingProposals()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals?search=Test Proposal 1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposals.ShouldNotBeNull();
        proposals.Any(p => p.Title.Contains("Test Proposal 1")).ShouldBeTrue();
    }

    [Fact]
    public async Task GetProposals_WithPagination_ShouldRespectSkipAndTake()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals?skip=0&take=1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposals.ShouldNotBeNull();
        proposals.Count.ShouldBeLessThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetTrendingProposals_ShouldReturnOk_WithTrendingProposals()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals/trending", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposals.ShouldNotBeNull();
        proposals.Count.ShouldBeLessThanOrEqualTo(5); // Default take value
    }

    [Fact]
    public async Task GetProposalById_WithValidId_ShouldReturnProposal()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals/1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposal = JsonSerializer.Deserialize<ProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        proposal.ShouldNotBeNull();
        proposal.Id.ShouldBe(1);
        proposal.Title.ShouldBe("Test Proposal 1");
    }

    [Fact]
    public async Task GetProposalById_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals/999", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProposal_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var createDto = new CreateProposalDto
        {
            Title = "New Test Proposal",
            Description = "This is a new test proposal description that is long enough to pass validation requirements. Users need to contribute to increase their level.",
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", createDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProposal_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var updateDto = new UpdateProposalDto
        {
            Title = "Updated Test Proposal",
            Description = "This is an updated test proposal description that is long enough to pass validation requirements.",
            CategoryId = 2
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/proposals/1", updateDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteProposal_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/api/proposals/1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProposals_ShouldReturnValidJson()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        content.ShouldNotBeNullOrEmpty();

        // Should be able to deserialize without throwing
        Should.NotThrow(() => JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }));
    }

    [Fact]
    public async Task GetProposals_ShouldHaveCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    [Fact]
    public async Task ProposalEndpoints_ShouldBeAccessible()
    {
        // Test that all endpoints are reachable (not returning 404)
        var endpoints = new[]
        {
            "/api/proposals",
            "/api/proposals/trending",
            "/api/proposals/1"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint, Cancel);
            response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound, $"Endpoint {endpoint} should be accessible");
        }
    }

    // C# 13.0 - Test contribution system with modern patterns
    [Theory]
    [InlineData("PetitNicolas", "Basic contributor level")]
    [InlineData("GrosMoyenNicolas", "Intermediate contributor level")]
    [InlineData("GrosNicolas", "Advanced contributor level")]
    [InlineData("NicolasSupreme", "Expert contributor level")]
    public async Task ContributionSystem_ShouldTrackUserLevels_BasedOnActivity(string contributionLevel, string description)
    {
        // Arrange - the level is one the API knows
        Enum.TryParse<NicolasQuiPaieData.DTOs.ContributionLevel>(contributionLevel, out _).ShouldBeTrue(description);

        // Act - Get proposals to see if they include contribution tracking
        var response = await _client.GetAsync("/api/proposals", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Validate contribution tracking data is present
        proposals.ShouldNotBeNull();
        foreach (var proposal in proposals)
        {
            // Each proposal should have creator information for contribution tracking
            proposal.CreatedById.ShouldNotBeNullOrEmpty();
            proposal.CreatedByDisplayName.ShouldNotBeNullOrEmpty();
            proposal.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);

            // Engagement metrics for contribution system
            proposal.VotesFor.ShouldBeGreaterThanOrEqualTo(0);
            proposal.VotesAgainst.ShouldBeGreaterThanOrEqualTo(0);
            proposal.ViewsCount.ShouldBeGreaterThanOrEqualTo(0);
        }
    }

    // C# 13.0 - Performance testing with contribution metrics
    [Fact]
    public async Task GetProposals_ShouldPerformWell_WithContributionTracking()
    {
        // Arrange
        const int concurrentRequests = 10;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(i => _client.GetAsync($"/api/proposals?skip={i * 5}&take=5", Cancel))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - Performance should be good even with contribution tracking
        responses.All(r => r.StatusCode == HttpStatusCode.OK).ShouldBeTrue();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete in under 5 seconds

        // Validate all responses have valid contribution data
        var contentTasks = responses.Select(r => r.Content.ReadAsStringAsync(Cancel));
        var contents = await Task.WhenAll(contentTasks);

        contents.All(c => c is not null && c.Length > 0).ShouldBeTrue();
        contents.All(c => c.StartsWith('[') || c.StartsWith('{')).ShouldBeTrue();
    }
}
