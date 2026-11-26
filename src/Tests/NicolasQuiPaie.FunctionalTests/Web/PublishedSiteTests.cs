namespace NicolasQuiPaie.FunctionalTests.Web;

/// <summary>The front as published: read-only mode on, served by NicolasQuiPaieWeb.Host.</summary>
public class PublishedSiteTests(Platform platform) : FrontTest(platform)
{
    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Home_ShouldPresentThePlatform()
    {
        // Act
        await OpenAsync("/");

        // Assert
        await Expect.That(Page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Nicolas Qui Paie"));
        await Expect.That(Page.Locator(".hero-section")).ToBeVisibleAsync();
        await Expect.That(Page.Locator(".hero-section h1")).ToContainTextAsync("Nicolas");
        await Expect.That(Page.Locator(".nicolas-navbar")).ToContainTextAsync("Propositions");
    }

    [Theory]
    [InlineData("Propositions", "/proposals")]
    [InlineData("Trending", "/trending")]
    [InlineData("À Propos", "/about")]
    public async Task Menu_ShouldLeadToEachSection(string link, string path)
    {
        // Arrange
        await OpenAsync("/");
        var menuLink = Page.Locator($".nicolas-navbar a.nav-link[href='{path}']");
        await Expect.That(menuLink).ToContainTextAsync(link);

        // Act
        await menuLink.ClickAsync();

        // Assert
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == path);
        await Expect.That(Page.Locator("main h1, main h2").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task ClientRoute_ShouldBeServedByTheHost_WhenTypedDirectly()
    {
        // Act: an address only the WebAssembly router knows, typed in the address bar
        var response = await Page.GotoAsync(new Uri(Platform.FrontAddress, "/help/voting-system").ToString());
        await Page.Locator(".nicolas-navbar").WaitForAsync();

        // Assert
        response.ShouldNotBeNull().Status.ShouldBe(200);
        Address.ShouldBe("/help/voting-system");
        await Expect.That(Page.Locator("main .card").First).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Login_ShouldOfferToCreateAnAccount()
    {
        // Arrange
        await OpenAsync("/login");

        // Act
        await Page.GetByRole(AriaRole.Link, new() { Name = "Créer un compte Nicolas" }).ClickAsync();

        // Assert
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/register");
        await Expect.That(Page.Locator("#displayName")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Host_ShouldAnswerItsHealthProbe_AndSendDemoToTheCurtain()
    {
        // Arrange
        using var host = Platform.CreateFrontClient();

        // Act
        var health = await host.GetAsync("/health", Cancel);
        var demo = await host.GetAsync("/demo", Cancel);

        // Assert
        health.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await health.Content.ReadAsStringAsync(Cancel)).ShouldContain("Healthy");
        demo.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        demo.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe("/?demo");
    }
}
