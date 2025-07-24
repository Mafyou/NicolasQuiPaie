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
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? search = null) =>
        {
            var proposals = await proposalService.GetActiveProposalsAsync(skip, take, categoryId, search);
            return Results.Ok(proposals);
        })
        .WithName("GetProposals")
        .WithSummary("Récupère les propositions actives")
        .Produces<IEnumerable<ProposalDto>>();

        // GET /api/proposals/trending
        group.MapGet("/trending", async (
            [FromServices] IProposalService proposalService,
            [FromQuery] int take = 5) =>
        {
            var proposals = await proposalService.GetTrendingProposalsAsync(take);
            return Results.Ok(proposals);
        })
        .WithName("GetTrendingProposals")
        .WithSummary("Récupère les propositions tendances")
        .Produces<IEnumerable<ProposalDto>>();

        // GET /api/proposals/{id}
        group.MapGet("/{id:int}", async (
            [FromServices] IProposalService proposalService,
            int id) =>
        {
            var proposal = await proposalService.GetProposalByIdAsync(id);
            return proposal != null ? Results.Ok(proposal) : Results.NotFound();
        })
        .WithName("GetProposalById")
        .WithSummary("Récupère une proposition par son ID")
        .Produces<ProposalDto>()
        .Produces(404);

        // POST /api/proposals
        group.MapPost("/", [Authorize] async (
            [FromServices] IProposalService proposalService,
            [FromBody] CreateProposalDto createDto,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var proposal = await proposalService.CreateProposalAsync(createDto, userId);
                return Results.Created($"/api/proposals/{proposal.Id}", proposal);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateProposal")
        .WithSummary("Crée une nouvelle proposition")
        .Produces<ProposalDto>(201)
        .Produces(400)
        .Produces(401);

        // POST /api/proposals/{id}/views
        group.MapPost("/{id:int}/views", async (
            [FromServices] IProposalService proposalService,
            int id) =>
        {
            try
            {
                await proposalService.IncrementViewsAsync(id);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("IncrementViews")
        .WithSummary("Incrémente le nombre de vues d'une proposition")
        .Produces(200)
        .Produces(400);

        // PUT /api/proposals/{id}
        group.MapPut("/{id:int}", [Authorize] async (
            [FromServices] IProposalService proposalService,
            int id,
            [FromBody] UpdateProposalDto updateDto,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var proposal = await proposalService.UpdateProposalAsync(id, updateDto, userId);
                return Results.Ok(proposal);
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateProposal")
        .WithSummary("Met à jour une proposition")
        .Produces<ProposalDto>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // DELETE /api/proposals/{id}
        group.MapDelete("/{id:int}", [Authorize] async (
            [FromServices] IProposalService proposalService,
            int id,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                await proposalService.DeleteProposalAsync(id, userId);
                return Results.NoContent();
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("DeleteProposal")
        .WithSummary("Supprime une proposition")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);
    }
}