// C# 13.0 - Enhanced Proposal Endpoints Integration Tests with latest language features

using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tests.NicolasQuiPaie.IntegrationTests.Fixtures;

namespace Tests.NicolasQuiPaie.IntegrationTests.Endpoints;

/// <summary>
/// C# 13.0 - Proposal endpoints integration tests using collection expressions, primary constructors, and modern patterns
/// </summary>
[TestFixture]
public class ProposalEndpointsTests
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

    // C# 13.0 - Record for endpoint test scenarios
    public record ProposalEndpointScenario(
        string Name,
        string Endpoint,
        HttpMethod Method,
        object? RequestBody = null,
        HttpStatusCode ExpectedStatus = HttpStatusCode.OK,
        bool RequiresAuth = false);

    // C# 13.0 - Collection expressions for comprehensive endpoint testing
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
                Description = "This is a comprehensive test proposal created through integration testing to validate the complete API functionality.",
                CategoryId = 1
            }, 
            HttpStatusCode.Created, true),
        new("Update proposal", "/api/proposals/1", HttpMethod.Put,
            new UpdateProposalDto
            {
                Title = "Updated Integration Test Proposal",
                Description = "This proposal has been updated through integration testing to validate the update functionality.",
                CategoryId = 2
            },
            HttpStatusCode.OK, true),
        new("Delete proposal", "/api/proposals/1", HttpMethod.Delete, null, HttpStatusCode.NoContent, true)
    ];

    [Test]
    [TestCaseSource(nameof(PublicEndpointScenarios))]
    public async Task PublicProposalEndpoints_ShouldWork_WithoutAuthentication(ProposalEndpointScenario scenario)
    {
        // Act
        var request = new HttpRequestMessage(scenario.Method, scenario.Endpoint);
        if (scenario.RequestBody is not null)
        {
            request.Content = JsonContent.Create(scenario.RequestBody);
        }

        var response = await _client.SendAsync(request);

        // Assert - C# 13.0 enhanced validation with modern null checking
        response.StatusCode.ShouldBe(scenario.ExpectedStatus);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();

            // Validate JSON structure
            Should.NotThrow(() => JsonDocument.Parse(content));
        }
    }

    [Test]
    public async Task GetProposals_ShouldReturnValidStructure_WithCorrectPagination()
    {
        // Arrange - C# 13.0 collection expressions for pagination tests
        var paginationScenarios = new[]
        {
            new { Skip = 0, Take = 5, Description = "First page" },
            new { Skip = 5, Take = 5, Description = "Second page" },
            new { Skip = 0, Take = 20, Description = "Large page" },
            new { Skip = 100, Take = 10, Description = "Beyond available data" }
        };

        foreach (var scenario in paginationScenarios)
        {
            // Act
            var response = await _client.GetAsync($"/api/proposals?skip={scenario.Skip}&take={scenario.Take}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<ProposalDto[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // C# 13.0 - Modern null checking
            proposals.ShouldNotBeNull();
            proposals.Length.ShouldBeLessThanOrEqualTo(scenario.Take);

            // Validate proposal structure with modern null patterns
            foreach (var proposal in proposals)
            {
                proposal.Id.ShouldBeGreaterThan(0);
                proposal.Title.ShouldNotBeNullOrEmpty();
                proposal.Description.ShouldNotBeNullOrEmpty();
                proposal.Status.ShouldBe(DtoProposalStatus.Active);
                proposal.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            }
        }
    }

    [Test]
    public async Task GetProposals_ShouldFilterCorrectly_ByCategory()
    {
        // Arrange - Test different categories
        int[] categoriesToTest = [1, 2, 3];

        foreach (var categoryId in categoriesToTest)
        {
            // Act
            var response = await _client.GetAsync($"/api/proposals?categoryId={categoryId}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<ProposalDto[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // C# 13.0 - Modern null checking
            proposals.ShouldNotBeNull();
            
            // All returned proposals should belong to the requested category
            proposals.All(p => p.CategoryId == categoryId).ShouldBeTrue();
        }
    }

    [Test]
    public async Task GetProposals_ShouldSearchCorrectly_ByTitle()
    {
        // Arrange - C# 13.0 collection expressions for search terms
        string[] searchTerms = ["Test", "Proposal", "1"];

        foreach (var searchTerm in searchTerms)
        {
            // Act
            var response = await _client.GetAsync($"/api/proposals?search={Uri.EscapeDataString(searchTerm)}");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<ProposalDto[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // C# 13.0 - Modern null checking
            proposals.ShouldNotBeNull();

            // All returned proposals should contain the search term (case-insensitive)
            foreach (var proposal in proposals)
            {
                var titleContains = proposal.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                var descriptionContains = proposal.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
                
                (titleContains || descriptionContains).ShouldBeTrue(
                    $"Proposal '{proposal.Title}' should contain search term '{searchTerm}'");
            }
        }
    }

    [Test]
    public async Task GetTrendingProposals_ShouldReturnMostPopular_InCorrectOrder()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals/trending");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var proposals = JsonSerializer.Deserialize<ProposalDto[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Modern null checking
        proposals.ShouldNotBeNull();
        proposals.Length.ShouldBeLessThanOrEqualTo(5); // Default trending limit

        // Proposals should be ordered by popularity (total votes or recent activity)
        for (int i = 1; i < proposals.Length; i++)
        {
            var previous = proposals[i - 1];
            var current = proposals[i];
            
            // Trending should be ordered by some popularity metric
            (previous.TotalVotes >= current.TotalVotes || 
             previous.ViewsCount >= current.ViewsCount ||
             previous.CreatedAt >= current.CreatedAt).ShouldBeTrue();
        }
    }

    [Test]
    public async Task GetProposalById_ShouldReturnCompleteDetails_ForValidId()
    {
        // Act
        var response = await _client.GetAsync("/api/proposals/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var proposal = JsonSerializer.Deserialize<ProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Enhanced object validation with modern null checking
        proposal.ShouldNotBeNull();
        proposal.Id.ShouldBe(1);
        proposal.Title.ShouldNotBeNullOrEmpty();
        proposal.Description.ShouldNotBeNullOrEmpty();
        proposal.Status.ShouldBe(DtoProposalStatus.Active);
        proposal.CategoryId.ShouldBeGreaterThan(0);
        proposal.CreatedById.ShouldNotBeNullOrEmpty();
        proposal.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        
        // Validate computed properties
        proposal.TotalVotes.ShouldBe(proposal.VotesFor + proposal.VotesAgainst);
        if (proposal.TotalVotes > 0)
        {
            proposal.ApprovalRate.ShouldBeBetween(0, 100);
        }
    }

    // C# 13.0 - Authentication helper with modern JWT and null checking
    private async Task<string> GetTestAuthToken(string userId = "test-user-1")
    {
        // This would typically call the auth endpoint, but for integration tests
        // we can create a test token directly
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, $"{userId}@test.com"),
            new Claim("DisplayName", $"Test User {userId}"),
            new Claim("FiscalLevel", "PetitNicolas"),
            new Claim("ReputationScore", "100"),
            new Claim("IsVerified", "true")
        };

        // Create a test JWT token (simplified for testing)
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes("MySecretKeyForNicolasQuiPaie2024TestingPurposes123!");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Test]
    public async Task CreateProposal_ShouldWork_WithValidAuthentication()
    {
        // Arrange
        var token = await GetTestAuthToken();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateProposalDto
        {
            Title = "Authenticated Test Proposal",
            Description = "This is a test proposal created with proper authentication to validate the complete create flow in integration testing.",
            CategoryId = 1,
            Tags = "test,integration,authentication"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var createdProposal = JsonSerializer.Deserialize<ProposalDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // C# 13.0 - Modern null checking
        createdProposal.ShouldNotBeNull();
        createdProposal.Title.ShouldBe(createDto.Title);
        createdProposal.Description.ShouldBe(createDto.Description);
        createdProposal.CategoryId.ShouldBe(createDto.CategoryId);
        createdProposal.Status.ShouldBe(DtoProposalStatus.Active);
        createdProposal.CreatedById.ShouldBe("test-user-1");
    }

    [Test]
    public async Task CreateProposal_ShouldFail_WithoutAuthentication()
    {
        // Arrange - Clear any existing authentication
        _client.DefaultRequestHeaders.Authorization = null;

        var createDto = new CreateProposalDto
        {
            Title = "Unauthenticated Test Proposal",
            Description = "This proposal should not be created without authentication.",
            CategoryId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", createDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // C# 13.0 - Collection expressions for validation error testing with modern null patterns
    [Test]
    [TestCase("", "Valid description that meets minimum length requirements", 1, HttpStatusCode.BadRequest)]
    [TestCase("Short", "Valid description that meets minimum length requirements", 1, HttpStatusCode.BadRequest)]
    [TestCase("Valid Title", "Too short", 1, HttpStatusCode.BadRequest)]
    [TestCase("Valid Title", "Valid description that meets minimum length requirements", 0, HttpStatusCode.BadRequest)]
    public async Task CreateProposal_ShouldValidateInput_AndReturnBadRequest(
        string title, string description, int categoryId, HttpStatusCode expectedStatus)
    {
        // Arrange
        var token = await GetTestAuthToken();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateProposalDto
        {
            Title = title,
            Description = description,
            CategoryId = categoryId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/proposals", createDto);

        // Assert
        response.StatusCode.ShouldBe(expectedStatus);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldContain("validation"); // Should contain validation error details
        }
    }

    // C# 13.0 - Performance testing with modern patterns and null checking
    [Test]
    public async Task GetProposals_ShouldPerformWell_WithLargeDataSets()
    {
        // Arrange
        const int concurrentRequests = 20;

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(i => _client.GetAsync($"/api/proposals?skip={i * 10}&take=10"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert - C# 13.0 enhanced performance validation with modern null checking
        responses.All(r => r.StatusCode == HttpStatusCode.OK).ShouldBeTrue();
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(10000); // Should complete in under 10 seconds

        // Validate all responses have valid content with modern null patterns
        var contentTasks = responses.Select(r => r.Content.ReadAsStringAsync());
        var contents = await Task.WhenAll(contentTasks);
        
        contents.All(c => c is not null && c.Length > 0).ShouldBeTrue();
        contents.All(c => c.StartsWith('[') || c.StartsWith('{')).ShouldBeTrue(); // Valid JSON arrays or objects
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up authentication headers between tests
        _client.DefaultRequestHeaders.Authorization = null;
    }
}