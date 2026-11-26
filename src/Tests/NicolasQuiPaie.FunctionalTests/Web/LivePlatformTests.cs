namespace NicolasQuiPaie.FunctionalTests.Web;

/// <summary>
/// The front with read-only mode off, talking to the local API: what a citizen does on the open platform, checked
/// on the page and then in the API.
/// </summary>
public class LivePlatformTests(Platform platform) : FrontTest(platform)
{
    protected override bool ReadOnlyMode => false;

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Proposals_ShouldListTheProposalsOfTheApi_AndOpenOne()
    {
        // Arrange
        using var api = Platform.CreateApiClient();
        var proposals = (await api.GetFromJsonAsync<List<ProposalDto>>("/api/proposals?take=100", Cancel)).ShouldNotBeNull();
        await OpenAsync("/proposals");
        var link = Page.Locator("a[href^='/proposal/']").First;
        await Expect.That(link).ToBeVisibleAsync();
        var title = (await link.InnerTextAsync()).Trim();

        // Act
        await link.ClickAsync();

        // Assert: a proposal of the API, its page and its vote buttons
        proposals.Select(proposal => proposal.Title).ShouldContain(title);
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath.StartsWith("/proposal/"));
        await Expect.That(Page.Locator("main h1")).ToHaveTextAsync(title);
        await Expect.That(Page.Locator(".voting-component")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Visitor_ShouldBeAskedToSignIn_BeforeVoting()
    {
        // Arrange
        var proposalId = await FirstProposalIdAsync();
        string? alert = null;
        Page.Dialog += async (_, dialog) =>
        {
            alert = dialog.Message;
            await dialog.DismissAsync();
        };
        await OpenAsync($"/proposal/{proposalId}");

        // Act
        await Page.Locator(".voting-for button").ClickAsync();

        // Assert
        await Expect.That(Page.Locator(".voting-component")).ToBeVisibleAsync();
        alert.ShouldBe("Vous devez être connecté pour voter");
    }

    [Fact]
    public async Task NewCitizen_ShouldSignUp_VoteAndComment()
    {
        // Arrange
        var email = $"front-{Guid.NewGuid():N}@test.com";
        await RegisterAsync(email, "Nicolas du Front");
        var proposalId = await FirstProposalIdAsync();
        await OpenAsync($"/proposal/{proposalId}");
        await Expect.That(Page.Locator(".voting-component")).ToBeVisibleAsync();

        // Act: a vote for, then a comment
        await Page.Locator(".voting-for button").ClickAsync();
        await Expect.That(Page.Locator(".voting-option.voting-for")).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("selected"));
        await Page.GetByRole(AriaRole.Button, new() { Name = "Commenter" }).ClickAsync();
        await Page.FillAsync("textarea", "Voté depuis le parcours du front.");
        // @bind updates on change: the text counts once the field is left
        await Page.PressAsync("textarea", "Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Publier" }).ClickAsync();

        // Assert: on the page, then in the API
        await Expect.That(Page.Locator(".comments-list")).ToContainTextAsync("Voté depuis le parcours du front.");
        using var api = Platform.CreateApiClient();
        var login = await api.PostAsJsonAsync("/api/auth/login", new LoginRequestDto { Email = email, Password = "Nicolas123" }, Cancel);
        var session = (await login.Content.ReadFromJsonAsync<AuthResponseDto>(Cancel)).ShouldNotBeNull();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        var vote = await api.GetFromJsonAsync<VoteDto>($"/api/votes/user/{session.User.Id}/proposal/{proposalId}", Cancel);
        vote.ShouldNotBeNull().VoteType.ShouldBe(VoteType.For);
    }

    [Fact]
    public async Task Citizen_ShouldSignIn_AndBeRefusedWithAWrongPassword()
    {
        // Arrange: an account made through the API
        using var api = Platform.CreateApiClient();
        var email = $"login-{Guid.NewGuid():N}@test.com";
        (await api.PostAsJsonAsync("/api/auth/register",
            new RegisterRequestDto { Email = email, Password = "Nicolas123", DisplayName = "Nicolas Connecté" }, Cancel)).EnsureSuccessStatusCode();
        await OpenAsync("/login");

        // Act: a wrong password first
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#password", "PasLeBon123");
        await Page.ClickAsync("form button[type=submit]");

        // Assert
        await Expect.That(Page.Locator(".card .alert-danger")).ToContainTextAsync("Invalid email or password");
        Address.ShouldBe("/login");

        // Act: the right one
        await Page.FillAsync("#password", "Nicolas123");
        await Page.ClickAsync("form button[type=submit]");

        // Assert
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
        await Expect.That(Page.Locator(".nicolas-navbar")).ToContainTextAsync("Nicolas Connecté");
    }

    private async Task<int> FirstProposalIdAsync()
    {
        using var api = Platform.CreateApiClient();
        var proposals = await api.GetFromJsonAsync<List<ProposalDto>>("/api/proposals", Cancel);
        return proposals.ShouldNotBeNull().First().Id;
    }
}
