namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Who may change a comment: its author, or an administrator (moderation).
/// </summary>
public static class CommentPermissionsExtensions
{
    extension(ClaimsPrincipal user)
    {
        /// <summary>The author of the comment may edit it, and so may any administrator.</summary>
        public bool CanEditComment(Comment comment)
            => user.IsCommentAuthor(comment) || user.IsInRole(NicolasRoles.Admin);

        /// <summary>The signed-in user wrote this comment.</summary>
        public bool IsCommentAuthor(Comment comment)
            => user.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } userId && userId == comment.UserId;
    }
}
