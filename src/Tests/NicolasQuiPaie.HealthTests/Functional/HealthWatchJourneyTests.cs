namespace NicolasQuiPaie.HealthTests.Functional;

/// <summary>
/// What Mafyou IT's health watch does every 15 minutes, against each app on a real loopback port: a plain GET of
/// /health, the JSON read with its status compared case-insensitively. Nothing of the app is shared but the address.
/// </summary>
public class HealthWatchJourneyTests
{
    /// <summary>What the watch reads of a report (MafyouAF's ApiHealth).</summary>
    private sealed record WatchedHealth(string Status, DateTime Timestamp);

    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    private static bool IsUp(WatchedHealth? report) => report?.Status.Equals("healthy", StringComparison.OrdinalIgnoreCase) == true;

    [Fact]
    public async Task TheWatch_SeesTheApiUp_AndItsWelcome()
    {
        // Arrange
        await using var api = new ApiApp();
        using var watch = new HttpClient { BaseAddress = Loopback.Start(api) };

        // Act
        using var response = await watch.GetAsync("/health", Cancel);
        var report = await response.Content.ReadFromJsonAsync<WatchedHealth>(Cancel);
        var welcome = await watch.GetStringAsync("/", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        IsUp(report).ShouldBeTrue();
        welcome.ShouldBe("Welcome to Nicolas Qui Paie API.");
    }

    [Fact]
    public async Task TheWatch_SeesTheApiDown_WhenItsDatabaseIs()
    {
        // Arrange
        await using var api = new ApiApp(databaseDown: true);
        using var watch = new HttpClient { BaseAddress = Loopback.Start(api) };

        // Act
        using var response = await watch.GetAsync("/health", Cancel);
        var report = await response.Content.ReadFromJsonAsync<WatchedHealth>(Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        IsUp(report).ShouldBeFalse();
        report.ShouldNotBeNull().Status.ShouldBe("Unhealthy");
    }

    [Fact]
    public async Task TheWatch_SeesTheSiteUp_AsAReport()
    {
        // Arrange
        await using var site = new HostApp();
        using var watch = new HttpClient { BaseAddress = Loopback.Start(site) };

        // Act
        var report = await watch.GetFromJsonAsync<WatchedHealth>("/health", Cancel);

        // Assert
        IsUp(report).ShouldBeTrue();
    }
}
