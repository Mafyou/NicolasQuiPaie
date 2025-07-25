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
            [FromServices] ILogger<Program> logger,
            int proposalId) =>
        {
            try
            {
                if (proposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for comments retrieval: {ProposalId}", proposalId);
                    return Results.BadRequest("Invalid proposal ID");
                }

                var comments = await commentRepository.GetCommentsForProposalAsync(proposalId);
                var commentsList = comments.ToList();
                
                if (commentsList.Count == 0)
                {
                    logger.LogWarning("No comments found for proposal: {ProposalId}", proposalId);
                }

                var commentDtos = commentsList.Select(c => new CommentDto
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
                }).ToList();

                return Results.Ok(commentDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving comments for proposal: {ProposalId}", proposalId);
                return Results.Problem("Error retrieving comments");
            }
        })
        .WithName("GetCommentsForProposal")
        .WithSummary("Récupère les commentaires d'une proposition")
        .Produces<IEnumerable<CommentDto>>()
        .Produces(400)
        .Produces(500);

        // POST /api/comments
        group.MapPost("/", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
            [FromServices] ILogger<Program> logger,
            [FromBody] CreateCommentDto createDto,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized comment creation attempt for proposal {ProposalId} from IP: {ClientIP}", 
                        createDto.ProposalId, clientIp);
                    return Results.Unauthorized();
                }

                if (string.IsNullOrWhiteSpace(createDto.Content))
                {
                    logger.LogWarning("Comment creation with empty content by user {UserId} for proposal {ProposalId}", 
                        userId, createDto.ProposalId);
                    return Results.BadRequest("Comment content is required");
                }

                if (createDto.Content.Length > 1000)
                {
                    logger.LogWarning("Comment creation with content too long ({Length} chars) by user {UserId} for proposal {ProposalId}", 
                        createDto.Content.Length, userId, createDto.ProposalId);
                    return Results.BadRequest("Comment content must be 1000 characters or less");
                }

                if (createDto.ProposalId <= 0)
                {
                    logger.LogWarning("Invalid proposal ID for comment creation: {ProposalId} by user {UserId}", 
                        createDto.ProposalId, userId);
                    return Results.BadRequest("Invalid proposal ID");
                }

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
            catch (ArgumentException ex)
            {
                logger.LogError(ex, "Invalid arguments for comment creation by user {UserId} for proposal {ProposalId}", 
                    userId, createDto.ProposalId);
                return Results.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error creating comment by user {UserId} for proposal {ProposalId}", 
                    userId, createDto.ProposalId);
                return Results.Problem("Error creating comment");
            }
        })
        .WithName("CreateComment")
        .WithSummary("Créer un nouveau commentaire")
        .Produces<CommentDto>(201)
        .Produces(400)
        .Produces(401)
        .Produces(500);

        // PUT /api/comments/{id}
        group.MapPut("/{id:int}", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
            [FromServices] ILogger<Program> logger,
            int id,
            [FromBody] UpdateCommentDto updateDto,
            ClaimsPrincipal user,
            HttpContext context) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var clientIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    logger.LogWarning("Unauthorized comment update attempt for comment {CommentId} from IP: {ClientIP}", 
                        id, clientIp);
                    return Results.Unauthorized();
                }

                if (id <= 0)
                {
                    logger.LogWarning("Invalid comment ID for update: {CommentId} by user {UserId}", id, userId);
                    return Results.BadRequest("Invalid comment ID");
                }

                if (string.IsNullOrWhiteSpace(updateDto.Content))
                {
                    logger.LogWarning("Comment update with empty content by user {UserId} for comment {CommentId}", 
                        userId, id);
                    return Results.BadRequest("Comment content is required");
                }

                var comment = await commentRepository.GetByIdAsync(id);
                if (comment == null)
                {
                    logger.LogWarning("Comment not found for update: {CommentId} by user {UserId}", id, userId);
                    return Results.NotFound();
                }

                if (comment.UserId != userId)
                {
                    logger.LogWarning("Unauthorized comment update: user {UserId} trying to update comment {CommentId} owned by {OwnerUserId}", 
                        userId, id, comment.UserId);
                    return Results.Forbid();
                }

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
                logger.LogError(ex, "Critical error updating comment {CommentId} by user {UserId}", id, userId);
                return Results.Problem("Error updating comment");
            }
        })
        .WithName("UpdateComment")
        .WithSummary("Modifier un commentaire")
        .Produces<CommentDto>()
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);

        // DELETE /api/comments/{id}
        group.MapDelete("/{id:int}", [Authorize] async (
            [FromServices] ICommentRepository commentRepository,
            [FromServices] IUnitOfWork unitOfWork,
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
                    logger.LogWarning("Unauthorized comment deletion attempt for comment {CommentId} from IP: {ClientIP}", 
                        id, clientIp);
                    return Results.Unauthorized();
                }

                if (id <= 0)
                {
                    logger.LogWarning("Invalid comment ID for deletion: {CommentId} by user {UserId}", id, userId);
                    return Results.BadRequest("Invalid comment ID");
                }

                var comment = await commentRepository.GetByIdAsync(id);
                if (comment == null)
                {
                    logger.LogWarning("Comment not found for deletion: {CommentId} by user {UserId}", id, userId);
                    return Results.NotFound();
                }

                if (comment.UserId != userId)
                {
                    logger.LogWarning("Unauthorized comment deletion: user {UserId} trying to delete comment {CommentId} owned by {OwnerUserId}", 
                        userId, id, comment.UserId);
                    return Results.Forbid();
                }

                comment.IsDeleted = true;
                comment.UpdatedAt = DateTime.UtcNow;

                await commentRepository.UpdateAsync(comment);
                await unitOfWork.SaveChangesAsync();

                // Warning level for comment deletion as it's an important action
                logger.LogWarning("Comment deleted: {CommentId} by user {UserId} from IP: {ClientIP}", 
                    id, userId, clientIp);

                return Results.NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error deleting comment {CommentId} by user {UserId}", id, userId);
                return Results.Problem("Error deleting comment");
            }
        })
        .WithName("DeleteComment")
        .WithSummary("Supprimer un commentaire")
        .Produces(204)
        .Produces(400)
        .Produces(401)
        .Produces(403)
        .Produces(404)
        .Produces(500);
    }
}