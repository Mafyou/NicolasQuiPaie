namespace NicolasQuiPaieAPI.Presentation.Endpoints;

public static class CommentEndpoints
{
    public static void MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/comments")
            .WithTags("Comments")
            .WithOpenApi();

        // GET /api/comments/proposal/{proposalId}
        group.MapGet("/proposal/{proposalId:int}", async (
            [FromServices] ICommentRepository commentRepository,
            int proposalId) =>
        {
            try
            {
                var comments = await commentRepository.GetCommentsForProposalAsync(proposalId);
                var commentDtos = comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    LikesCount = c.LikesCount,
                    UserId = c.UserId,
                    UserDisplayName = c.User?.DisplayName ?? "Anonymous",
                    ProposalId = c.ProposalId,
                    ParentCommentId = c.ParentCommentId,
                    IsDeleted = c.IsDeleted
                });
                return Results.Ok(commentDtos);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error retrieving comments: {ex.Message}");
            }
        })
        .WithName("GetCommentsForProposal")
        .WithSummary("Récupère les commentaires d'une proposition")
        .Produces<IEnumerable<CommentDto>>();

        // POST /api/comments
        group.MapPost("/", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
            [FromBody] CreateCommentDto createDto,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var comment = new NicolasQuiPaieAPI.Infrastructure.Models.Comment
                {
                    Content = createDto.Content,
                    UserId = userId,
                    ProposalId = createDto.ProposalId,
                    ParentCommentId = createDto.ParentCommentId,
                    CreatedAt = DateTime.UtcNow,
                    LikesCount = 0,
                    IsDeleted = false
                };

                var addedComment = await commentRepository.AddAsync(comment);
                await unitOfWork.SaveChangesAsync();

                var commentDto = new CommentDto
                {
                    Id = addedComment.Id,
                    Content = addedComment.Content,
                    CreatedAt = addedComment.CreatedAt,
                    UpdatedAt = addedComment.UpdatedAt,
                    LikesCount = addedComment.LikesCount,
                    UserId = addedComment.UserId,
                    UserDisplayName = user.FindFirst("DisplayName")?.Value ?? "Anonymous",
                    ProposalId = addedComment.ProposalId,
                    ParentCommentId = addedComment.ParentCommentId,
                    IsDeleted = addedComment.IsDeleted
                };

                return Results.Created($"/api/comments/{commentDto.Id}", commentDto);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("CreateComment")
        .WithSummary("Créer un nouveau commentaire")
        .Produces<CommentDto>(201)
        .Produces(400)
        .Produces(401);

        // PUT /api/comments/{id}
        group.MapPut("/{id:int}", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
            int id,
            [FromBody] UpdateCommentDto updateDto,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var comment = await commentRepository.GetByIdAsync(id);
                if (comment == null)
                    return Results.NotFound();

                if (comment.UserId != userId)
                    return Results.Forbid();

                comment.Content = updateDto.Content;
                comment.UpdatedAt = DateTime.UtcNow;

                await commentRepository.UpdateAsync(comment);
                await unitOfWork.SaveChangesAsync();

                var commentDto = new CommentDto
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    LikesCount = comment.LikesCount,
                    UserId = comment.UserId,
                    UserDisplayName = user.FindFirst("DisplayName")?.Value ?? "Anonymous",
                    ProposalId = comment.ProposalId,
                    ParentCommentId = comment.ParentCommentId,
                    IsDeleted = comment.IsDeleted
                };

                return Results.Ok(commentDto);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("UpdateComment")
        .WithSummary("Modifier un commentaire")
        .Produces<CommentDto>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);

        // DELETE /api/comments/{id}
        group.MapDelete("/{id:int}", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
            int id,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            try
            {
                var comment = await commentRepository.GetByIdAsync(id);
                if (comment == null)
                    return Results.NotFound();

                if (comment.UserId != userId)
                    return Results.Forbid();

                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;

                await commentRepository.UpdateAsync(comment);
                await unitOfWork.SaveChangesAsync();

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        })
        .WithName("DeleteComment")
        .WithSummary("Supprimer un commentaire")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404);
    }
}