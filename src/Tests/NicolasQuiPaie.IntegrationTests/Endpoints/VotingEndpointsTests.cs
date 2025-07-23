using NicolasQuiPaie.IntegrationTests.Fixtures;
using System.Net;
using System.Text.Json;
using ContributionLevel = NicolasQuiPaieAPI.Infrastructure.Models.ContributionLevel;
using VoteType = NicolasQuiPaieData.DTOs.VoteType;

namespace NicolasQuiPaie.IntegrationTests.Endpoints;

[TestFixture]
public class VotingEndpointsTests
{
    private NicolasQuiPaieApiFactory _factory = null!;
    private HttpClient _client = null!;
    private const string AuthenticatedUserId = "test-user-1";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new NicolasQuiPaieApiFactory();
        _client = _factory.CreateClient();
        
        // Try to initialize test data, but don't fail if it doesn't work
        var initialized = await _factory.InitializeTestDataAsync();
        if (!initialized)
        {
            Console.WriteLine("Warning: Test data initialization failed, some tests may not work correctly");
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task PostVote_ShouldReturnCreated_WhenValidVoteData()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = VoteType.For,
            Comment = "Je soutiens cette proposition pour les Nicolas français"
        };

        // Add authentication header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var voteResult = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        voteResult.ShouldNotBeNull();
        voteResult.ProposalId.ShouldBe(1);
        voteResult.VoteType.ShouldBe(VoteType.For);
        voteResult.UserId.ShouldBe(AuthenticatedUserId);
        voteResult.Weight.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task PostVote_ShouldReturnBadRequest_WhenInvalidProposalId()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 999, // Non-existent proposal
            VoteType = VoteType.For,
            Comment = "Vote sur proposition inexistante"
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostVote_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = VoteType.For
        };

        // Remove authentication header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task PostVote_ShouldUpdateExistingVote_WhenUserVotesAgain()
    {
        // Arrange - First vote
        var firstVoteDto = new CreateVoteDto
        {
            ProposalId = 2,
            VoteType = VoteType.For,
            Comment = "Premier vote pour"
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act - Cast first vote
        var firstResponse = await _client.PostAsJsonAsync("/api/votes", firstVoteDto);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Arrange - Second vote (different choice)
        var secondVoteDto = new CreateVoteDto
        {
            ProposalId = 2,
            VoteType = VoteType.Against,
            Comment = "J'ai changé d'avis, je vote contre"
        };

        // Act - Cast second vote
        var secondResponse = await _client.PostAsJsonAsync("/api/votes", secondVoteDto);

        // Assert
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK); // Updated, not created

        var content = await secondResponse.Content.ReadAsStringAsync();
        var updatedVote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        updatedVote.ShouldNotBeNull();
        updatedVote.VoteType.ShouldBe(VoteType.Against);
        updatedVote.Comment.ShouldBe("J'ai changé d'avis, je vote contre");
    }

    [Test]
    public async Task GetVotesForProposal_ShouldReturnOk_WithVotesList()
    {
        // Arrange
        var proposalId = 1;

        // Act
        var response = await _client.GetAsync($"/api/votes/proposal/{proposalId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        votes.ShouldNotBeNull();
        votes.All(v => v.ProposalId == proposalId).ShouldBeTrue();
    }

    [Test]
    public async Task GetVotesForProposal_ShouldReturnNotFound_WhenProposalDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/votes/proposal/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetUserVoteForProposal_ShouldReturnOk_WhenUserHasVoted()
    {
        // Arrange - First cast a vote
        var voteDto = new CreateVoteDto
        {
            ProposalId = 3,
            VoteType = VoteType.For,
            Comment = "Mon vote de test"
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        await _client.PostAsJsonAsync("/api/votes", voteDto);

        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/3");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userVote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userVote.ShouldNotBeNull();
        userVote.UserId.ShouldBe(AuthenticatedUserId);
        userVote.ProposalId.ShouldBe(3);
        userVote.VoteType.ShouldBe(VoteType.For);
    }

    [Test]
    public async Task GetUserVoteForProposal_ShouldReturnNotFound_WhenUserHasNotVoted()
    {
        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUserVote_ShouldReturnNoContent_WhenVoteExists()
    {
        // Arrange - First cast a vote
        var voteDto = new CreateVoteDto
        {
            ProposalId = 4,
            VoteType = VoteType.Against,
            Comment = "Vote à supprimer"
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        await _client.PostAsJsonAsync("/api/votes", voteDto);

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/4");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify vote was actually deleted
        var getResponse = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/4");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUserVote_ShouldReturnNotFound_WhenVoteDoesNotExist()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/999");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUserVote_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetUserVotes_ShouldReturnOk_WithUserVotesList()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        votes.ShouldNotBeNull();
        votes.All(v => v.UserId == AuthenticatedUserId).ShouldBeTrue();
    }

    [Test]
    public async Task PostVote_ShouldCalculateCorrectWeight_BasedOnFiscalLevel()
    {
        // Test different fiscal levels and their vote weights
        var testCases = new[]
        {
            new { Token = GetTestJwtToken(ContributionLevel.PetitNicolas), ExpectedWeight = 1 },
            new { Token = GetTestJwtToken(ContributionLevel.GrosMoyenNicolas), ExpectedWeight = 2 },
            new { Token = GetTestJwtToken(ContributionLevel.GrosNicolas), ExpectedWeight = 3 },
            new { Token = GetTestJwtToken(ContributionLevel.NicolasSupreme), ExpectedWeight = 5 }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var voteDto = new CreateVoteDto
            {
                ProposalId = 5, // Use same proposal for all tests
                VoteType = VoteType.For,
                Comment = $"Vote avec poids {testCase.ExpectedWeight}"
            };

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", testCase.Token);

            // Act
            var response = await _client.PostAsJsonAsync("/api/votes", voteDto);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            var content = await response.Content.ReadAsStringAsync();
            var vote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            vote.ShouldNotBeNull();
            vote.Weight.ShouldBe(testCase.ExpectedWeight);
        }
    }

    [Test]
    public async Task PostVote_ShouldValidateVoteType()
    {
        // Arrange
        var invalidVoteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = (VoteType)999, // Invalid vote type
            Comment = "Vote invalide"
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", invalidVoteDto);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostVote_ShouldAcceptOptionalComment()
    {
        // Arrange - Vote without comment
        var voteWithoutComment = new CreateVoteDto
        {
            ProposalId = 6,
            VoteType = VoteType.For
            // No comment
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteWithoutComment);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var vote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        vote.ShouldNotBeNull();
        vote.Comment.ShouldBeNullOrEmpty();
    }

    [Test]
    public async Task PostVote_ShouldValidateCommentLength()
    {
        // Arrange - Vote with overly long comment
        var voteWithLongComment = new CreateVoteDto
        {
            ProposalId = 7,
            VoteType = VoteType.Against,
            Comment = new string('A', 1001) // Exceeds maximum length
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetTestJwtToken());

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteWithLongComment);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #region Helper Methods

    /// <summary>
    /// Generates a test JWT token for authentication
    /// </summary>
    private static string GetTestJwtToken(ContributionLevel fiscalLevel = ContributionLevel.PetitNicolas)
    {
        // This would typically generate a real JWT token for testing
        // For now, return a mock token that your test authentication handler recognizes
        return $"test-token-{AuthenticatedUserId}-{fiscalLevel}";
    }

    #endregion
}