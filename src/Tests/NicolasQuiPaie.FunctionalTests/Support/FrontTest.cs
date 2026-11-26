using System.Text.Json.Nodes;

namespace NicolasQuiPaie.FunctionalTests.Support;

/// <summary>
/// A journey in the browser, in a context of its own (fresh storage). The front reads its API address from
/// appsettings*.json: those files are served as published, with the address of the local API in place of the
/// production one. Any request for an azurewebsites.net address is stopped and fails the test.
/// </summary>
public abstract class FrontTest(Platform platform) : IAsyncLifetime
{
    private readonly List<string> _productionCalls = [];

    protected Platform Platform { get; } = platform;

    protected IBrowserContext Context { get; private set; } = null!;

    protected IPage Page { get; private set; } = null!;

    /// <summary>Path and query of the page on display.</summary>
    protected string Address => new Uri(Page.Url).PathAndQuery;

    /// <summary>Whether the front is told it is in read-only mode, as the published settings say.</summary>
    protected virtual bool ReadOnlyMode => true;

    public virtual async ValueTask InitializeAsync()
    {
        Context = await Platform.Browser.NewContextAsync(new() { ViewportSize = new() { Width = 1440, Height = 900 }, Locale = "fr-FR" });
        Context.SetDefaultTimeout(30_000);

        await Context.RouteAsync("**/*.azurewebsites.net/**", async route =>
        {
            _productionCalls.Add(route.Request.Url);
            await route.AbortAsync();
        });
        // The bodies are made before the page opens: a route handler that waits on the browser can stall it.
        var settingsFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using (var host = Platform.CreateFrontClient())
        {
            foreach (var file in new[] { "appsettings.json", "appsettings.Development.json" })
            {
                using var response = await host.GetAsync(file, TestContext.Current.CancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    settingsFiles[file] = WithLocalApi(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
                }
            }
        }

        await Context.RouteAsync("**/appsettings*.json", route =>
            settingsFiles.TryGetValue(Path.GetFileName(new Uri(route.Request.Url).AbsolutePath), out var body)
                ? route.FulfillAsync(new() { Status = 200, Body = body, ContentType = "application/json; charset=utf-8" })
                : route.ContinueAsync());

        Page = await Context.NewPageAsync();
    }

    /// <summary>A settings file of the front, pointed at the local API, with the read-only mode of the test.</summary>
    private string WithLocalApi(string published)
    {
        var settings = JsonNode.Parse(published.TrimStart('\uFEFF'))!.AsObject();
        if (settings["ApiSettings"] is JsonObject api)
        {
            api["BaseUrl"] = Platform.ApiAddress.ToString().TrimEnd('/');
        }

        if (settings["MaintenanceSettings"] is JsonObject maintenance)
        {
            maintenance["IsReadOnlyMode"] = ReadOnlyMode;
        }

        return settings.ToJsonString();
    }

    public virtual async ValueTask DisposeAsync()
    {
        await Context.CloseAsync();
        _productionCalls.ShouldBeEmpty("The front must never call the production API from the tests");
        GC.SuppressFinalize(this);
    }

    /// <summary>Opens a page of the front and waits for Blazor to have rendered the layout.</summary>
    protected async Task OpenAsync(string path)
    {
        await Page.GotoAsync(new Uri(Platform.FrontAddress, path).ToString());
        await Page.Locator(".nicolas-navbar").WaitForAsync();
    }

    /// <summary>Signs up through the registration page and waits for the home page the front then opens.</summary>
    protected async Task RegisterAsync(string email, string displayName, string password = "Nicolas123")
    {
        await OpenAsync("/register");
        await Page.FillAsync("#email", email);
        await Page.FillAsync("#displayName", displayName);
        await Page.FillAsync("#password", password);
        await Page.ClickAsync("form button[type=submit]");
        await Page.WaitForURLAsync(url => new Uri(url).AbsolutePath == "/");
        await Expect.That(Page.Locator(".nicolas-navbar")).ToContainTextAsync(displayName);
    }
}

/// <summary>Playwright's web-first assertions, with the timeout of the journeys.</summary>
public static class Expect
{
    public static ILocatorAssertions That(ILocator locator) => Assertions.Expect(locator);

    public static IPageAssertions That(IPage page) => Assertions.Expect(page);
}
