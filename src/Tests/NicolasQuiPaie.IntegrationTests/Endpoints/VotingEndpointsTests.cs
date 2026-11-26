
namespace NicolasQuiPaie.IntegrationTests.Endpoints;

public class VotingEndpointsTests(NicolasQuiPaieApiFactory factory) : IClassFixture<NicolasQuiPaieApiFactory>, IDisposable
{
    private const string AuthenticatedUserId = NicolasQuiPaieApiFactory.TestUserId;

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Signs the client's requests with a real token of a seeded user.</summary>
    private async Task AuthenticateAsync(string userId = NicolasQuiPaieApiFactory.TestUserId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await factory.CreateTokenAsync(userId));

    [Fact]
    public async Task PostVote_ShouldReturnCreated_WhenValidVoteData()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = DtoVoteType.For,
            Comment = "Je soutiens cette proposition pour les Nicolas français"
        };

        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var voteResult = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        voteResult.ShouldNotBeNull();
        voteResult.ProposalId.ShouldBe(1);
        voteResult.VoteType.ShouldBe(DtoVoteType.For);
        voteResult.UserId.ShouldBe(AuthenticatedUserId);
        voteResult.Weight.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task PostVote_ShouldReturnBadRequest_WhenInvalidProposalId()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 999, // Non-existent proposal
            VoteType = DtoVoteType.For,
            Comment = "Vote sur proposition inexistante"
        };

        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostVote_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        var voteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = DtoVoteType.For
        };

        // Remove authentication header
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostVote_ShouldUpdateExistingVote_WhenUserVotesAgain()
    {
        // Arrange - First vote
        var firstVoteDto = new CreateVoteDto
        {
            ProposalId = 2,
            VoteType = DtoVoteType.For,
            Comment = "Premier vote pour"
        };

        await AuthenticateAsync();

        // Act - Cast first vote
        var firstResponse = await _client.PostAsJsonAsync("/api/votes", firstVoteDto, Cancel);
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Arrange - Second vote (different choice)
        var secondVoteDto = new CreateVoteDto
        {
            ProposalId = 2,
            VoteType = DtoVoteType.Against,
            Comment = "J'ai changé d'avis, je vote contre"
        };

        // Act - Cast second vote
        var secondResponse = await _client.PostAsJsonAsync("/api/votes", secondVoteDto, Cancel);

        // Assert
        // The API answers 201 for a new vote and for a changed one alike: what matters is the vote it sends back.
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await secondResponse.Content.ReadAsStringAsync(Cancel);
        var updatedVote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        updatedVote.ShouldNotBeNull();
        updatedVote.VoteType.ShouldBe(DtoVoteType.Against);
        updatedVote.Comment.ShouldBe("J'ai changé d'avis, je vote contre");
    }

    [Fact]
    public async Task GetVotesForProposal_ShouldReturnOk_WithVotesList()
    {
        // Arrange
        var proposalId = 1;

        // Act
        var response = await _client.GetAsync($"/api/votes/proposal/{proposalId}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        votes.ShouldNotBeNull();
        votes.All(v => v.ProposalId == proposalId).ShouldBeTrue();
    }

    [Fact]
    public async Task GetVotesForProposal_ShouldReturnAnEmptyList_WhenProposalDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/votes/proposal/999", Cancel);

        // Assert: the API answers with no votes rather than a 404
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var votes = await response.Content.ReadFromJsonAsync<List<VoteDto>>(Cancel);
        votes.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task GetUserVoteForProposal_ShouldReturnOk_WhenUserHasVoted()
    {
        // Arrange - First cast a vote
        var voteDto = new CreateVoteDto
        {
            ProposalId = 3,
            VoteType = DtoVoteType.For,
            Comment = "Mon vote de test"
        };

        await AuthenticateAsync();

        await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/3", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var userVote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userVote.ShouldNotBeNull();
        userVote.UserId.ShouldBe(AuthenticatedUserId);
        userVote.ProposalId.ShouldBe(3);
        userVote.VoteType.ShouldBe(DtoVoteType.For);
    }

    [Fact]
    public async Task GetUserVoteForProposal_ShouldReturnNotFound_WhenUserHasNotVoted()
    {
        // Arrange: a user may only read their own vote, signed in
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/999", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserVote_ShouldReturnNoContent_WhenVoteExists()
    {
        // Arrange - First cast a vote
        var voteDto = new CreateVoteDto
        {
            ProposalId = 4,
            VoteType = DtoVoteType.Against,
            Comment = "Vote à supprimer"
        };

        await AuthenticateAsync();

        await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/4", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify vote was actually deleted
        var getResponse = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/4", Cancel);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserVote_ShouldReturnNotFound_WhenVoteDoesNotExist()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/999", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUserVote_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.DeleteAsync($"/api/votes/user/{AuthenticatedUserId}/proposal/1", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserVotes_ShouldReturnOk_WithUserVotesList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync($"/api/votes/user/{AuthenticatedUserId}", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var votes = JsonSerializer.Deserialize<List<VoteDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        votes.ShouldNotBeNull();
        votes.All(v => v.UserId == AuthenticatedUserId).ShouldBeTrue();
    }

    // One Nicolas, one vote: the contribution level is a badge, it never changes the weight of a vote.
    [Theory]
    [InlineData(ContributionLevel.PetitNicolas)]
    [InlineData(ContributionLevel.GrosMoyenNicolas)]
    [InlineData(ContributionLevel.GrosNicolas)]
    [InlineData(ContributionLevel.NicolasSupreme)]
    public async Task PostVote_ShouldGiveTheSameWeight_WhateverTheContributionLevel(ContributionLevel contributionLevel)
    {
        // Arrange
        var userId = await factory.AddUserAsync($"voter-{contributionLevel}", contributionLevel);
        await AuthenticateAsync(userId);
        var voteDto = new CreateVoteDto
        {
            ProposalId = 5,
            VoteType = DtoVoteType.For,
            Comment = $"Vote d'un {contributionLevel}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var vote = await response.Content.ReadFromJsonAsync<VoteDto>(Cancel);
        vote.ShouldNotBeNull();
        vote.UserId.ShouldBe(userId);
        vote.Weight.ShouldBe(1);
    }

    [Fact]
    public async Task PostVote_ShouldValidateVoteType()
    {
        // Arrange
        var invalidVoteDto = new CreateVoteDto
        {
            ProposalId = 1,
            VoteType = (DtoVoteType)999, // Invalid vote type
            Comment = "Vote invalide"
        };

        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", invalidVoteDto, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostVote_ShouldAcceptOptionalComment()
    {
        // Arrange - Vote without comment
        var voteWithoutComment = new CreateVoteDto
        {
            ProposalId = 6,
            VoteType = DtoVoteType.For
            // No comment
        };

        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteWithoutComment, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync(Cancel);
        var vote = JsonSerializer.Deserialize<VoteDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        vote.ShouldNotBeNull();
        vote.Comment.ShouldBeNullOrEmpty();
    }

    [Fact]
    public async Task PostVote_ShouldValidateCommentLength()
    {
        // Arrange - Vote with overly long comment
        var voteWithLongComment = new CreateVoteDto
        {
            ProposalId = 7,
            VoteType = DtoVoteType.Against,
            Comment = new string('A', 1001) // Exceeds maximum length
        };

        await AuthenticateAsync();

        // Act
        var response = await _client.PostAsJsonAsync("/api/votes", voteWithLongComment, Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

}