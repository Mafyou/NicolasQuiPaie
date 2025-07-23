// C# 13.0 - Enhanced Analytics Endpoints Integration Tests with latest language features

using Tests.NicolasQuiPaie.IntegrationTests.Fixtures;

namespace Tests.NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>
/// C# 13.0 - Analytics endpoints integration tests using collection expressions and modern patterns
/// </summary>
[TestFixture]
public class AnalyticsEndpointsTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new NicolasQuiPaieApiFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    // C# 13.0 - Record for analytics test expectations
    public record AnalyticsEndpointTest(
        string Endpoint,
        string Description,
        HttpStatusCode ExpectedStatus = HttpStatusCode.OK,
        string? RequiredProperty = null);

    // C# 13.0 - Collection expressions for endpoint test definitions
    public static readonly AnalyticsEndpointTest[] AnalyticsEndpoints =
    [
        new("/api/analytics/global-stats", "Global statistics endpoint"),
        new("/api/analytics/dashboard-stats", "Dashboard statistics endpoint"),
        new("/api/analytics/voting-trends", "Voting trends endpoint"),
        new("/api/analytics/fiscal-distribution", "Fiscal level distribution endpoint"),
        new("/api/analytics/top-contributors", "Top contributors endpoint"),
        new("/api/analytics/frustration-barometer", "Frustration barometer endpoint")
    ];

    [Test]
    [TestCaseSource(nameof(AnalyticsEndpoints))]
    public async Task AnalyticsEndpoint_ShouldReturnValidResponse(AnalyticsEndpointTest endpointTest)
    {
        // Act
        var response = await _client.GetAsync(endpointTest.Endpoint);

        // Assert - C# 13.0 enhanced validation with modern null checking
        response.StatusCode.ShouldBe(endpointTest.ExpectedStatus);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();

        // Validate JSON structure
        var jsonDocument = JsonDocument.Parse(content);
        jsonDocument.RootElement.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Test]
    public async Task GlobalStatsEndpoint_ShouldReturnExpectedStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/global-stats");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var stats = JsonSerializer.Deserialize<GlobalStatsDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Enhanced object validation with modern null checking
        stats.ShouldNotBeNull();
        stats.TotalUsers.ShouldBeGreaterThanOrEqualTo(0);
        stats.TotalProposals.ShouldBeGreaterThanOrEqualTo(0);
        stats.TotalVotes.ShouldBeGreaterThanOrEqualTo(0);
        stats.TotalComments.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task DashboardStatsEndpoint_ShouldReturnComprehensiveMetrics()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/dashboard-stats");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var dashboard = JsonSerializer.Deserialize<DashboardStatsDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Pattern matching for validation with modern null checking
        dashboard.ShouldNotBeNull();
        dashboard.TotalUsers.ShouldBeGreaterThanOrEqualTo(0);
        dashboard.ActiveProposals.ShouldBeGreaterThanOrEqualTo(0);
        dashboard.TotalVotes.ShouldBeGreaterThanOrEqualTo(0);
        dashboard.RasLebolMeter.ShouldBeBetween(0, 100);
    }

    [Test]
    public async Task VotingTrendsEndpoint_ShouldReturnTimeSeriesData()
    {
        // Arrange - C# 13.0 collection expressions for time periods
        string[] timePeriodsToTest = ["7d", "30d", "90d", "1y"];

        foreach (var period in timePeriodsToTest)
        {
            // Act
            var response = await _client.GetAsync($"/api/analytics/voting-trends?period={period}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var trends = JsonSerializer.Deserialize<VotingTrendsDto[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // C# 13.0 - Modern null checking
            trends.ShouldNotBeNull();
            trends.Length.ShouldBeGreaterThanOrEqualTo(0);

            // Validate trend data structure
            foreach (var trend in trends)
            {
                trend.Date.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
                trend.VotesCount.ShouldBeGreaterThanOrEqualTo(0);
            }
        }
    }

    [Test]
    public async Task FiscalDistributionEndpoint_ShouldReturnAllFiscalLevels()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/fiscal-distribution");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var distribution = JsonSerializer.Deserialize<FiscalDistributionDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Enhanced collection validation with modern null checking
        distribution.ShouldNotBeNull();
        distribution.Length.ShouldBeGreaterThan(0);

        // Validate fiscal level coverage
        var expectedLevels = new[]
        {
            "PetitNicolas",
            "GrosMoyenNicolas", 
            "GrosNicolas",
            "NicolasSupreme"
        };

        foreach (var level in expectedLevels)
        {
            distribution.Any(d => d.FiscalLevel == level).ShouldBeTrue(
                $"Distribution should include {level}");
        }

        // Validate percentage calculation
        var totalPercentage = distribution.Sum(d => d.Percentage);
        Math.Abs(totalPercentage - 100.0).ShouldBeLessThan(0.1); // Allow for rounding
    }

    [Test]
    public async Task TopContributorsEndpoint_ShouldReturnRankedUsers()
    {
        // Arrange - C# 13.0 collection expressions for different limits
        int[] limitsToTest = [5, 10, 20];

        foreach (var limit in limitsToTest)
        {
            // Act
            var response = await _client.GetAsync($"/api/analytics/top-contributors?limit={limit}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var contributors = JsonSerializer.Deserialize<TopContributorDto[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // C# 13.0 - Modern null checking
            contributors.ShouldNotBeNull();
            contributors.Length.ShouldBeLessThanOrEqualTo(limit);

            // Validate ranking order (should be descending by score)
            for (int i = 1; i < contributors.Length; i++)
            {
                contributors[i - 1].ReputationScore.ShouldBeGreaterThanOrEqualTo(
                    contributors[i].ReputationScore);
            }
        }
    }

    [Test]
    public async Task FrustrationBarometerEndpoint_ShouldReturnValidMeter()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/frustration-barometer");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var barometer = JsonSerializer.Deserialize<FrustrationBarometerDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Enhanced validation with pattern matching and modern null checking
        barometer.ShouldNotBeNull();
        barometer.CurrentLevel.ShouldBeBetween(0, 100);
        barometer.Trend.ShouldBeOneOf("increasing", "decreasing", "stable");
        barometer.LastUpdated.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    // C# 13.0 - Collection expressions for comprehensive endpoint testing
    [Test]
    public async Task AllAnalyticsEndpoints_ShouldBeAccessible_AndReturnValidJson()
    {
        // Arrange
        var endpoints = AnalyticsEndpoints.Select(e => e.Endpoint).ToArray();

        // Act & Assert - C# 13.0 parallel testing with modern null checking
        var tasks = endpoints.Select(async endpoint =>
        {
            var response = await _client.GetAsync(endpoint);
            response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound, 
                $"Endpoint {endpoint} should be accessible");
            
            var content = await response.Content.ReadAsStringAsync();
            Should.NotThrow(() => JsonDocument.Parse(content), 
                $"Endpoint {endpoint} should return valid JSON");
        });

        await Task.WhenAll(tasks);
    }

    // C# 13.0 - Modern error handling testing with pattern matching
    [Test]
    [TestCase("/api/analytics/invalid-endpoint", HttpStatusCode.NotFound)]
    [TestCase("/api/analytics/voting-trends?period=invalid", HttpStatusCode.BadRequest)]
    [TestCase("/api/analytics/top-contributors?limit=-1", HttpStatusCode.BadRequest)]
    [TestCase("/api/analytics/top-contributors?limit=1000", HttpStatusCode.BadRequest)]
    public async Task AnalyticsEndpoints_ShouldHandleInvalidRequests_Appropriately(
        string endpoint, HttpStatusCode expectedStatus)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.ShouldBe(expectedStatus);
    }

    // C# 13.0 - Performance testing with modern patterns
    [Test]
    public async Task AnalyticsEndpoints_ShouldPerformWell_UnderLoad()
    {
        // Arrange
        const int concurrentRequests = 10;
        var endpoint = "/api/analytics/dashboard-stats";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => _client.GetAsync(endpoint))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - C# 13.0 enhanced performance validation with modern null checking
        responses.All(r => r.StatusCode == HttpStatusCode.OK).ShouldBeTrue();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete in under 5 seconds
        
        // Validate response consistency with modern null checking
        var contents = await Task.WhenAll(responses.Select(r => r.Content.ReadAsStringAsync()));
        contents.All(c => c is not null && c.Length > 0).ShouldBeTrue();
    }

    // C# 13.0 - Data consistency testing with modern null patterns
    [Test]
    public async Task AnalyticsEndpoints_ShouldProvideConsistentData_AcrossMultipleCalls()
    {
        // Act - Call the same endpoint multiple times
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => _client.GetAsync("/api/analytics/global-stats"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.All(r => r.StatusCode == HttpStatusCode.OK).ShouldBeTrue();

        var statsResponses = await Task.WhenAll(responses.Select(async r =>
        {
            var content = await r.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<GlobalStatsDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }));

        // Data should be consistent across calls (unless there's concurrent modification)
        // C# 13.0 - Modern null checking with pattern matching
        var firstStats = statsResponses[0];
        firstStats.ShouldNotBeNull();
        
        foreach (var stats in statsResponses.Skip(1))
        {
            stats.ShouldNotBeNull();
            // Allow for small variations due to potential concurrent operations
            Math.Abs(stats.TotalUsers - firstStats.TotalUsers).ShouldBeLessThanOrEqualTo(1);
        }
    }
}