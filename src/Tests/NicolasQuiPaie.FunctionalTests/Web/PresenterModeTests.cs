namespace NicolasQuiPaie.FunctionalTests.Web;

/// <summary>
/// Presenter mode: /demo, typed in front of the audience, opens the home page under a curtain of four scenes
/// (wwwroot/js/demo.js), driven by the keyboard or a presentation remote; the curtain then lifts on a guided tour.
/// </summary>
public class PresenterModeTests(Platform platform) : FrontTest(platform)
{
    private ILocator Curtain => Page.Locator("dialog#demo-stage");

    private ILocator Tour => Page.Locator("aside.demo-tour");

    [Fact]
    public async Task Demo_ShouldOpenTheHomePageUnderTheCurtain()
    {
        // Act
        await Page.GotoAsync(new Uri(Platform.FrontAddress, "/demo").ToString());

        // Assert: the host's redirection, then the first scene
        await Expect.That(Curtain).ToBeVisibleAsync();
        await Expect.That(Page.Locator("#demo-title")).ToContainTextAsync("Qui Paie");
        await Expect.That(Curtain).ToHaveAttributeAsync("data-scene", "intro");
        Address.ShouldBe("/?demo=intro");
    }

    [Fact]
    public async Task Curtain_ShouldFollowTheRemote_ForwardAndBack()
    {
        // Arrange
        await Page.GotoAsync(new Uri(Platform.FrontAddress, "/demo").ToString());
        await Expect.That(Curtain).ToBeVisibleAsync();

        // Act & Assert: → and Page Down go forward, ← goes back
        await Page.Keyboard.PressAsync("ArrowRight");
        await Expect.That(Curtain).ToHaveAttributeAsync("data-scene", "principle");
        await Page.Keyboard.PressAsync("PageDown");
        await Expect.That(Curtain).ToHaveAttributeAsync("data-scene", "vote");
        await Expect.That(Page.Locator(".scene-vote")).ToContainTextAsync("× 1 voix");
        await Page.Keyboard.PressAsync("ArrowLeft");
        await Expect.That(Curtain).ToHaveAttributeAsync("data-scene", "principle");
        Address.ShouldBe("/?demo=principle");
    }

    [Fact]
    public async Task Escape_ShouldLiftTheCurtainOnTheSite_WithoutTheTour()
    {
        // Arrange
        await Page.GotoAsync(new Uri(Platform.FrontAddress, "/demo").ToString());
        await Expect.That(Curtain).ToBeVisibleAsync();

        // Act
        await Page.Keyboard.PressAsync("Escape");

        // Assert
        await Expect.That(Curtain).ToHaveCountAsync(0);
        await Expect.That(Tour).ToHaveCountAsync(0);
        await Expect.That(Page.Locator(".hero-section")).ToBeVisibleAsync();
        Address.ShouldBe("/");
    }

    [Fact]
    public async Task LastScene_ShouldLiftTheCurtainOnTheGuidedTour_WhichWalksThroughThePages()
    {
        // Arrange
        await Page.GotoAsync(new Uri(Platform.FrontAddress, "/demo").ToString());
        await Expect.That(Curtain).ToBeVisibleAsync();

        // Act: to the last scene, then once more
        await Page.Keyboard.PressAsync("End");
        await Expect.That(Curtain).ToHaveAttributeAsync("data-scene", "reveal");
        await Page.Keyboard.PressAsync("ArrowRight");

        // Assert: the tour starts on the home page, step one of ten
        await Expect.That(Curtain).ToHaveCountAsync(0);
        await Expect.That(Tour).ToBeVisibleAsync();
        await Expect.That(Tour.Locator("output")).ToHaveTextAsync("01/10");
        await Expect.That(Tour.Locator("h2")).ToHaveTextAsync("Voici Nicolas Qui Paie");

        // Act: four steps on, the tour moves to the proposals page and picks up there
        foreach (var expected in new[] { "02/10", "03/10", "04/10" })
        {
            await Page.Keyboard.PressAsync("ArrowRight");
            await Expect.That(Tour.Locator("output")).ToHaveTextAsync(expected);
        }

        await Page.Keyboard.PressAsync("ArrowRight");

        // Assert
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/proposals");
        await Expect.That(Tour.Locator("output")).ToHaveTextAsync("05/10");
        await Expect.That(Tour.Locator("h2")).ToHaveTextAsync("Les propositions");
    }

    [Fact]
    public async Task Home_ShouldShowNoCurtain_WithoutTheDemoAddress()
    {
        // Act
        await OpenAsync("/");

        // Assert
        await Expect.That(Page.Locator(".hero-section")).ToBeVisibleAsync();
        await Expect.That(Curtain).ToHaveCountAsync(0);
        await Expect.That(Tour).ToHaveCountAsync(0);
    }
}
