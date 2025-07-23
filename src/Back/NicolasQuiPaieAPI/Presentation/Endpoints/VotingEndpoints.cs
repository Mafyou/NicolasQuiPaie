namespace NicolasQuiPaieAPI.Presentation.Endpoints;

public static class VotingEndpoints
{
    public static void MapVotingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/votes")
            .WithTags("Voting")
            .WithOpenApi();

        // POST /api/votes
        group.MapPost("/", [Authorize] async (
            [FromServices] IVotingService votingService,
            [FromBody] CreateVoteDto voteDto,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var vote = await votingService.CastVoteAsync(voteDto, userId);
                return Results.Created($"/api/votes/{vote.Id}", vote);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CastVote")
        .WithSummary("Voter pour une proposition")
        .Produces<VoteDto>(201)
        .Produces(400)
        .Produces(401);

        // GET /api/votes/proposal/{proposalId}
        group.MapGet("/proposal/{proposalId:int}", async (
            [FromServices] IVotingService votingService,
            int proposalId) =>
        {
            var votes = await votingService.GetVotesForProposalAsync(proposalId);
            return Results.Ok(votes);
        })
        .WithName("GetVotesForProposal")
        .WithSummary("Récupère les votes d'une proposition")
        .Produces<IEnumerable<VoteDto>>();

        // GET /api/votes/user/{userId}/proposal/{proposalId}
        group.MapGet("/user/{userId}/proposal/{proposalId:int}", [Authorize] async (
            [FromServices] IVotingService votingService,
            string userId,
            int proposalId,
            ClaimsPrincipal user) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId)
                return Results.Forbid();

            var vote = await votingService.GetUserVoteForProposalAsync(userId, proposalId);
            return vote != null ? Results.Ok(vote) : Results.NotFound();
        })
        .WithName("GetUserVoteForProposal")
        .WithSummary("Récupère le vote d'un utilisateur pour une proposition")
        .Produces<VoteDto>()
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // DELETE /api/votes/user/{userId}/proposal/{proposalId}
        group.MapDelete("/user/{userId}/proposal/{proposalId:int}", [Authorize] async (
            [FromServices] IVotingService votingService,
            string userId,
            int proposalId,
            ClaimsPrincipal user) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId)
                return Results.Forbid();

            try
            {
                await votingService.RemoveVoteAsync(userId, proposalId);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("RemoveVote")
        .WithSummary("Supprimer le vote d'un utilisateur")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403);

        // GET /api/votes/user/{userId}
        group.MapGet("/user/{userId}", [Authorize] async (
            [FromServices] IVotingService votingService,
            string userId,
            ClaimsPrincipal user) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != userId)
                return Results.Forbid();

            var votes = await votingService.GetUserVotesAsync(userId);
            return Results.Ok(votes);
        })
        .WithName("GetUserVotes")
        .WithSummary("Récupère tous les votes d'un utilisateur")
        .Produces<IEnumerable<VoteDto>>()
        .Produces(401)
        .Produces(403);
    }
}