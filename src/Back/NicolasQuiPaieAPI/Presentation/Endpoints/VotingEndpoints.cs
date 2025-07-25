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
            [FromServices] ILogger<Program> logger,
            [FromBody] CreateVoteDto voteDto,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized vote attempt for proposal {ProposalId} from IP: {ClientIP}", 
                        voteDto.ProposalId, clientIp);
                    return Results.Unauthorized();
                }

                if (voteDto.ProposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for vote: {ProposalId} by user {UserId}", 
                        voteDto.ProposalId, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                var vote = await votingService.CastVoteAsync(voteDto, userId);
                return Results.Created($"/api/votes/{vote.Id}", vote);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Invalid vote operation for proposal {ProposalId} by user {UserId}: {Message}", 
                    voteDto.ProposalId, userId, ex.Message);
                return Results.BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid arguments for vote on proposal {ProposalId} by user {UserId}", 
                    voteDto.ProposalId, userId);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error casting vote for proposal {ProposalId} by user {UserId}", 
                    voteDto.ProposalId, userId);
                return Results.Problem("Error casting vote");
            }
        })
        .WithName("CastVote")
        .WithSummary("Voter pour une proposition")
        .Produces<VoteDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // GET /api/votes/proposal/{proposalId}
        group.MapGet("/proposal/{proposalId:int}", async (
            [FromServices] IVotingService votingService,
            [FromServices] ILogger<Program> logger,
            int proposalId) =>
        {
            try
            {
                if (proposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for votes retrieval: {ProposalId}", proposalId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                var votes = await votingService.GetVotesForProposalAsync(proposalId);
                var votesList = votes.ToList();

                if (votesList.Count == 0)
                {
                    logger.LogWarning("No votes found for proposal: {ProposalId}", proposalId);
                }

                return Results.Ok(votesList);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid arguments retrieving votes for proposal: {ProposalId}", proposalId);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving votes for proposal: {ProposalId}", proposalId);
                return Results.Problem("Error retrieving votes");
            }
        })
        .WithName("GetVotesForProposal")
        .WithSummary("Récupère les votes d'une proposition")
        .Produces<IEnumerable<VoteDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/votes/user/{userId}/proposal/{proposalId}
        group.MapGet("/user/{userId}/proposal/{proposalId:int}", [Authorize] async (
            [FromServices] IVotingService votingService,
            [FromServices] ILogger<Program> logger,
            string userId,
            int proposalId,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    logger.LogWarning("Unauthorized user vote retrieval attempt for user {TargetUserId}, proposal {ProposalId} from IP: {ClientIP}", 
                        userId, proposalId, clientIp);
                    return Results.Unauthorized();
                }

                if (currentUserId != userId)
                {
                    logger.LogWarning("Forbidden user vote access: user {CurrentUserId} trying to access votes of user {TargetUserId} for proposal {ProposalId}", 
                        currentUserId, userId, proposalId);
                    return Results.Forbid();
                }

                if (proposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for user vote retrieval: {ProposalId} by user {UserId}", 
                        proposalId, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                var vote = await votingService.GetUserVoteForProposalAsync(userId, proposalId);
                
                if (vote == null)
                {
                    logger.LogWarning("No vote found for user {UserId} on proposal {ProposalId}", userId, proposalId);
                    return Results.NotFound();
                }

                return Results.Ok(vote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user vote: user {UserId}, proposal {ProposalId}", userId, proposalId);
                return Results.Problem("Error retrieving user vote");
            }
        })
        .WithName("GetUserVoteForProposal")
        .WithSummary("Récupère le vote d'un utilisateur pour une proposition")
        .Produces<VoteDto>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // DELETE /api/votes/user/{userId}/proposal/{proposalId}
        group.MapDelete("/user/{userId}/proposal/{proposalId:int}", [Authorize] async (
            [FromServices] IVotingService votingService,
            [FromServices] ILogger<Program> logger,
            string userId,
            int proposalId,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    logger.LogWarning("Unauthorized vote removal attempt for user {TargetUserId}, proposal {ProposalId} from IP: {ClientIP}", 
                        userId, proposalId, clientIp);
                    return Results.Unauthorized();
                }

                if (currentUserId != userId)
                {
                    logger.LogWarning("Forbidden vote removal: user {CurrentUserId} trying to remove vote of user {TargetUserId} for proposal {ProposalId}", 
                        currentUserId, userId, proposalId);
                    return Results.Forbid();
                }

                if (proposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for vote removal: {ProposalId} by user {UserId}", 
                        proposalId, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                await votingService.RemoveVoteAsync(userId, proposalId);

                // Warning level for vote removal as it's an important action
                logger.LogWarning("Vote removed: user {UserId} for proposal {ProposalId} from IP: {ClientIP}", 
                    userId, proposalId, clientIp);

                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Vote not found for removal: user {UserId}, proposal {ProposalId}", userId, proposalId);
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error removing vote: user {UserId}, proposal {ProposalId}", userId, proposalId);
                return Results.Problem("Error removing vote");
            }
        })
        .WithName("RemoveVote")
        .WithSummary("Supprimer le vote d'un utilisateur")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // GET /api/votes/user/{userId}
        group.MapGet("/user/{userId}", [Authorize] async (
            [FromServices] IVotingService votingService,
            [FromServices] ILogger<Program> logger,
            string userId,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var currentUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(currentUserId))
                {
                    logger.LogWarning("Unauthorized user votes retrieval attempt for user {TargetUserId} from IP: {ClientIP}", 
                        userId, clientIp);
                    return Results.Unauthorized();
                }

                if (currentUserId != userId)
                {
                    logger.LogWarning("Forbidden user votes access: user {CurrentUserId} trying to access votes of user {TargetUserId}", 
                        currentUserId, userId);
                    return Results.Forbid();
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    logger.LogWarning("Empty user ID for votes retrieval by user {CurrentUserId}", currentUserId);
                    return Results.BadRequest("Invalid user ID");
                }

                var votes = await votingService.GetUserVotesAsync(userId);
                var votesList = votes.ToList();

                if (votesList.Count == 0)
                {
                    logger.LogWarning("No votes found for user: {UserId}", userId);
                }

                return Results.Ok(votesList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving votes for user: {UserId}", userId);
                return Results.Problem("Error retrieving user votes");
            }
        })
        .WithName("GetUserVotes")
        .WithSummary("Récupère tous les votes d'un utilisateur")
        .Produces<IEnumerable<VoteDto>>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(500);
    }
}