using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NicolasQuiPaie.IntegrationTests.Fixtures;
using NicolasQuiPaieData.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace NicolasQuiPaie.IntegrationTests.Controllers
{
    [TestFixture]
    public class ProposalEndpointsTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

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

        [Test]
        public async Task GetProposals_ShouldReturnOk_WithProposals()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
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

        [Test]
        public async Task GetProposals_WithCategoryFilter_ShouldReturnFilteredProposals()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals?categoryId=1");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            proposals.ShouldNotBeNull();
            proposals.All(p => p.CategoryId == 1).ShouldBeTrue();
        }

        [Test]
        public async Task GetProposals_WithSearch_ShouldReturnMatchingProposals()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals?search=Test Proposal 1");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            proposals.ShouldNotBeNull();
            proposals.Any(p => p.Title.Contains("Test Proposal 1")).ShouldBeTrue();
        }

        [Test]
        public async Task GetProposals_WithPagination_ShouldRespectSkipAndTake()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals?skip=0&take=1");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            proposals.ShouldNotBeNull();
            proposals.Count.ShouldBeLessThanOrEqualTo(1);
        }

        [Test]
        public async Task GetTrendingProposals_ShouldReturnOk_WithTrendingProposals()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals/trending");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var proposals = JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            proposals.ShouldNotBeNull();
            proposals.Count.ShouldBeLessThanOrEqualTo(5); // Default take value
        }

        [Test]
        public async Task GetProposalById_WithValidId_ShouldReturnProposal()
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

            proposal.ShouldNotBeNull();
            proposal.Id.ShouldBe(1);
            proposal.Title.ShouldBe("Test Proposal 1");
        }

        [Test]
        public async Task GetProposalById_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals/999");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task CreateProposal_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Arrange
            var createDto = new CreateProposalDto
            {
                Title = "New Test Proposal",
                Description = "This is a new test proposal description that is long enough to pass validation requirements.",
                CategoryId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/proposals", createDto);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Test]
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
            var response = await _client.PutAsJsonAsync("/api/proposals/1", updateDto);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task DeleteProposal_WithoutAuthentication_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.DeleteAsync("/api/proposals/1");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Test]
        public async Task GetProposals_ShouldReturnValidJson()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.ShouldNotBeNullOrEmpty();

            // Should be able to deserialize without throwing
            Should.NotThrow(() => JsonSerializer.Deserialize<List<ProposalDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }));
        }

        [Test]
        public async Task GetProposals_ShouldHaveCorrectContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/proposals");

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
        }

        [Test]
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
                var response = await _client.GetAsync(endpoint);
                response.StatusCode.ShouldNotBe(HttpStatusCode.NotFound, $"Endpoint {endpoint} should be accessible");
            }
        }
    }
}