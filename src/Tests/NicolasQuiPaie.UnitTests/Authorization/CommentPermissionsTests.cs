namespace NicolasQuiPaie.UnitTests.Authorization;

[TestFixture]
public class CommentPermissionsTests
{
    private static readonly Comment CommentOfAlice = new() { Id = 1, UserId = "alice", Content = "Mon commentaire" };

    private static ClaimsPrincipal SignedIn(string userId, params string[] roles)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), .. roles.Select(role => new Claim(ClaimTypes.Role, role))],
            "Test"));

    [Test]
    public void TheAuthorWithoutAdminRole_CanEditTheirComment()
        => SignedIn("alice", NicolasRoles.User).CanEditComment(CommentOfAlice).ShouldBeTrue();

    [Test]
    public void AnAdmin_CanEditSomeoneElsesComment()
        => SignedIn("bob", NicolasRoles.Admin).CanEditComment(CommentOfAlice).ShouldBeTrue();

    [Test]
    public void AnAdmin_CanEditTheirOwnComment()
        => SignedIn("alice", NicolasRoles.User, NicolasRoles.Admin).CanEditComment(CommentOfAlice).ShouldBeTrue();

    [TestCase(NicolasRoles.User)]
    [TestCase(NicolasRoles.SuperUser)]
    [TestCase(NicolasRoles.Developer)]
    public void AnotherUserWhoIsNotAdmin_CannotEditTheComment(string role)
        => SignedIn("bob", role).CanEditComment(CommentOfAlice).ShouldBeFalse();

    [Test]
    public void AnAnonymousCaller_CannotEditTheComment()
        => new ClaimsPrincipal(new ClaimsIdentity()).CanEditComment(CommentOfAlice).ShouldBeFalse();

    [Test]
    public void AnEmptyUserId_IsNotTheAuthorOfACommentWithAnEmptyUserId()
        => SignedIn("").IsCommentAuthor(new Comment { UserId = "" }).ShouldBeFalse();
}
