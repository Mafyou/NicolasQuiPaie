namespace NicolasQuiPaieAPI.Presentation.Endpoints;

public static class ProposalEndpoints
{
    public static void MapProposalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proposals")
            .WithTags("Proposals")
            .WithOpenApi();

        // GET /api/proposals
        group.MapGet("/", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null) =>
        {
            try
            {
                // Validation with Warning logs
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                if (skip < 0)
                {
                    logger.LogWarning("Invalid skip parameter: {Skip}. Must be >= 0", skip);
                    return Results.BadRequest("Skip parameter must be >= 0");
                }

                var proposals = await proposalService.GetActiveProposalsAsync(skip, take, categoryId, search);
                var proposalsList = proposals.ToList();

                if (proposalsList.Count == 0)
                {
                    logger.LogWarning("No proposals found for filters: skip={Skip}, take={Take}, categoryId={CategoryId}, search={Search}", 
                        skip, take, categoryId, search);
                }

                return Results.Ok(proposalsList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving proposals: skip={Skip}, take={Take}, categoryId={CategoryId}, search={Search}", 
                    skip, take, categoryId, search);
                return Results.Problem("Error retrieving proposals");
            }
        })
        .WithName("GetProposals")
        .WithSummary("Récupère les propositions actives")
        .Produces<IEnumerable<ProposalDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/proposals/recent
        group.MapGet("/recent", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null) =>
        {
            try
            {
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter for recent proposals: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                if (skip < 0)
                {
                    logger.LogWarning("Invalid skip parameter for recent proposals: {Skip}. Must be >= 0", skip);
                    return Results.BadRequest("Skip parameter must be >= 0");
                }

                var proposals = await proposalService.GetRecentProposalsAsync(skip, take, category, search);
                var proposalsList = proposals.ToList();

                if (proposalsList.Count == 0)
                {
                    logger.LogWarning("No recent proposals found for filters: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                        skip, take, category, search);
                }

                return Results.Ok(proposalsList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving recent proposals: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                    skip, take, category, search);
                return Results.Problem("Error retrieving recent proposals");
            }
        })
        .WithName("GetRecentProposals")
        .WithSummary("Récupère les propositions les plus récentes")
        .Produces<IEnumerable<ProposalDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/proposals/popular
        group.MapGet("/popular", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null) =>
        {
            try
            {
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter for popular proposals: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                if (skip < 0)
                {
                    logger.LogWarning("Invalid skip parameter for popular proposals: {Skip}. Must be >= 0", skip);
                    return Results.BadRequest("Skip parameter must be >= 0");
                }

                var proposals = await proposalService.GetPopularProposalsAsync(skip, take, category, search);
                var proposalsList = proposals.ToList();

                if (proposalsList.Count == 0)
                {
                    logger.LogWarning("No popular proposals found for filters: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                        skip, take, category, search);
                }

                return Results.Ok(proposalsList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving popular proposals: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                    skip, take, category, search);
                return Results.Problem("Error retrieving popular proposals");
            }
        })
        .WithName("GetPopularProposals")
        .WithSummary("Récupère les propositions les plus populaires (nombre de votes positifs)")
        .Produces<IEnumerable<ProposalDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/proposals/controversial
        group.MapGet("/controversial", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null) =>
        {
            try
            {
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter for controversial proposals: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                if (skip < 0)
                {
                    logger.LogWarning("Invalid skip parameter for controversial proposals: {Skip}. Must be >= 0", skip);
                    return Results.BadRequest("Skip parameter must be >= 0");
                }

                var proposals = await proposalService.GetControversialProposalsAsync(skip, take, category, search);
                var proposalsList = proposals.ToList();

                if (proposalsList.Count == 0)
                {
                    logger.LogWarning("No controversial proposals found for filters: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                        skip, take, category, search);
                }

                return Results.Ok(proposalsList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving controversial proposals: skip={Skip}, take={Take}, category={Category}, search={Search}", 
                    skip, take, category, search);
                return Results.Problem("Error retrieving controversial proposals");
            }
        })
        .WithName("GetControversialProposals")
        .WithSummary("Récupère les propositions les plus controversées (équilibre votes pour/contre)")
        .Produces<IEnumerable<ProposalDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/proposals/trending
        group.MapGet("/trending", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int take = 5) =>
        {
            try
            {
                if (take <= 0 || take > 50)
                {
                    logger.LogWarning("Invalid take parameter for trending: {Take}. Must be between 1 and 50", take);
                    return Results.BadRequest("Take parameter must be between 1 and 50");
                }

                var proposals = await proposalService.GetTrendingProposalsAsync(take);
                var proposalsList = proposals.ToList();

                if (proposalsList.Count == 0)
                {
                    logger.LogWarning("No trending proposals found for take={Take}", take);
                }

                return Results.Ok(proposalsList);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving trending proposals: take={Take}", take);
                return Results.Problem("Error retrieving trending proposals");
            }
        })
        .WithName("GetTrendingProposals")
        .WithSummary("Récupère les propositions tendances")
        .Produces<IEnumerable<ProposalDto>>()
        .Produces(400)
        .Produces(500);

        // GET /api/proposals/{id}
        group.MapGet("/{id:int}", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            int id) =>
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid proposal ID: {ProposalId}. Must be > 0", id);
                    return Results.BadRequest("Invalid proposal ID");
                }

                var proposal = await proposalService.GetProposalByIdAsync(id);
                
                if (proposal == null)
                {
                    logger.LogWarning("Proposal not found: {ProposalId}", id);
                    return Results.NotFound();
                }

                return Results.Ok(proposal);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving proposal: {ProposalId}", id);
                return Results.Problem("Error retrieving proposal");
            }
        })
        .WithName("GetProposalById")
        .WithSummary("Récupère une proposition par son ID")
        .Produces<ProposalDto>()
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // POST /api/proposals
        group.MapPost("/", [Authorize] async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            [FromBody] CreateProposalDto createDto,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized proposal creation attempt from IP: {ClientIP}", clientIp);
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(createDto.Title))
                {
                    logger.LogWarning("Invalid proposal creation: empty title by user {UserId}", userId);
                    return Results.BadRequest("Title is required");
                }

                if (createDto.Title.Length > 200)
                {
                    logger.LogWarning("Invalid proposal creation: title too long ({Length} chars) by user {UserId}", 
                        createDto.Title.Length, userId);
                    return Results.BadRequest("Title must be 200 characters or less");
                }

                var proposal = await proposalService.CreateProposalAsync(createDto, userId);
                return Results.Created($"/api/proposals/{proposal.Id}", proposal);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid arguments for proposal creation by user {UserId}", userId);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error creating proposal by user {UserId}", userId);
                return Results.Problem("Error creating proposal");
            }
        })
        .WithName("CreateProposal")
        .WithSummary("Crée une nouvelle proposition")
        .Produces<ProposalDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // POST /api/proposals/{id}/views
        group.MapPost("/{id:int}/views", async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            int id) =>
        {
            try
            {
                if (id <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for view increment: {ProposalId}", id);
                    return Results.BadRequest("Invalid proposal ID");
                }

                await proposalService.IncrementViewsAsync(id);
                return Results.Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Proposal not found for view increment: {ProposalId}", id);
                return Results.NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error incrementing views for proposal: {ProposalId}", id);
                return Results.Problem("Error incrementing views");
            }
        })
        .WithName("IncrementViews")
        .WithSummary("Incrémente le nombre de vues d'une proposition")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);

        // PUT /api/proposals/{id}
        group.MapPut("/{id:int}", [Authorize] async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            int id,
            [FromBody] UpdateProposalDto updateDto,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized proposal update attempt for {ProposalId} from IP: {ClientIP}", id, clientIp);
                    return Results.Unauthorized();
                }

                if (id <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for update: {ProposalId} by user {UserId}", id, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                if (string.IsNullOrWhiteSpace(updateDto.Title))
                {
                    logger.LogWarning("Invalid proposal update: empty title for {ProposalId} by user {UserId}", id, userId);
                    return Results.BadRequest("Title is required");
                }

                var proposal = await proposalService.UpdateProposalAsync(id, updateDto, userId);
                return Results.Ok(proposal);
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Proposal not found for update: {ProposalId} by user {UserId}", id, userId);
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, "Unauthorized update attempt for proposal {ProposalId} by user {UserId}", id, userId);
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error updating proposal {ProposalId} by user {UserId}", id, userId);
                return Results.Problem("Error updating proposal");
            }
        })
        .WithName("UpdateProposal")
        .WithSummary("Met à jour une proposition")
        .Produces<ProposalDto>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // DELETE /api/proposals/{id}
        group.MapDelete("/{id:int}", [Authorize] async (
            [FromServices] IProposalService proposalService,
            [FromServices] ILogger<Program> logger,
            int id,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized proposal deletion attempt for {ProposalId} from IP: {ClientIP}", id, clientIp);
                    return Results.Unauthorized();
                }

                if (id <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for deletion: {ProposalId} by user {UserId}", id, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                await proposalService.DeleteProposalAsync(id, userId);
                
                // Warning level for deletion as it's an important action
                logger.LogWarning("Proposal deleted: {ProposalId} by user {UserId} from IP: {ClientIP}", id, userId, clientIp);
                
                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Proposal not found for deletion: {ProposalId} by user {UserId}", id, userId);
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, "Unauthorized deletion attempt for proposal {ProposalId} by user {UserId}", id, userId);
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error deleting proposal {ProposalId} by user {UserId}", id, userId);
                return Results.Problem("Error deleting proposal");
            }
        })
        .WithName("DeleteProposal")
        .WithSummary("Supprime une proposition")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);
    }
}