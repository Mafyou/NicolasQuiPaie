namespace NicolasQuiPaie.HealthTests.Integration;

/// <summary>/ and /health of the API and of the site's host, through WebApplicationFactory.</summary>
public class HealthEndpointTests
{
    private static CancellationToken Cancel => TestContext.Current.CancellationToken;

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancel));
        return json.RootElement.Clone();
    }

    [Fact]
    public async Task TheApi_AnswersTheStandardReport()
    {
        // Arrange
        await using var api = new ApiApp();
        using var client = api.CreateClient();

        // Act
        using var response = await client.GetAsync("/health", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        $"{response.Content.Headers.ContentType}".ShouldBe("application/json; charset=utf-8");
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        var report = await ReadAsync(response);
        HealthShape.NamesOf(report).ShouldBe(HealthShape.Report);
        report.GetProperty("status").GetString().ShouldBe("Healthy");
        report.GetProperty("service").GetString().ShouldBe("Nicolas Qui Paie API");
        report.GetProperty("environment").GetString().ShouldBe("Testing");
        var check = report.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        HealthShape.NamesOf(check).ShouldBe(HealthShape.Check);
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Healthy");
    }

    [Fact]
    public async Task TheApi_Answers503_WhenItsDatabaseIsDown_WithoutSayingWhere()
    {
        // Arrange
        await using var api = new ApiApp(databaseDown: true);
        using var client = api.CreateClient();

        // Act
        using var response = await client.GetAsync("/health", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        var text = await response.Content.ReadAsStringAsync(Cancel);
        text.ShouldNotContain("127.0.0.1");
        text.ShouldNotContain("nobody");
        var report = await ReadAsync(response);
        report.GetProperty("status").GetString().ShouldBe("Unhealthy");
        var check = report.GetProperty("checks")[0];
        check.GetProperty("status").GetString().ShouldBe("Unhealthy");
        check.GetProperty("description").GetString().ShouldBe("Database unreachable.");
    }

    [Fact]
    public async Task TheApiHealth_IsAnonymous_WhereTheRestAsksForASignIn()
    {
        // Arrange
        await using var api = new ApiApp();
        using var client = api.CreateClient();

        // Act
        using var analytics = await client.GetAsync("/api/analytics/global-stats", Cancel);
        using var anonymous = await client.GetAsync("/health", Cancel);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "made-up-token");
        using var withAnInvalidToken = await client.SendAsync(request, Cancel);

        // Assert
        analytics.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        anonymous.StatusCode.ShouldBe(HttpStatusCode.OK);
        withAnInvalidToken.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TheApiHealth_IsNeverRateLimited_NorCached()
    {
        // Arrange
        await using var api = new ApiApp();
        using var client = api.CreateClient();
        var timestamps = new HashSet<DateTime>();

        // Act: far more calls than any limit of Mafyou IT's servers lets through in a minute
        for (var call = 0; call < 120; call++)
        {
            using var response = await client.GetAsync("/health", Cancel);

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.OK, $"call {call + 1}");
            response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
            timestamps.Add((await ReadAsync(response)).GetProperty("timestamp").GetDateTime());
        }

        timestamps.Count.ShouldBeGreaterThan(1, "every call reads the health again");
    }

    [Fact]
    public async Task TheApiRoot_SaysWelcome_InPlainText()
    {
        // Arrange
        await using var api = new ApiApp();
        using var client = api.CreateClient();

        // Act
        using var response = await client.GetAsync("/", Cancel);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        $"{response.Content.Headers.ContentType}".ShouldBe("text/plain; charset=utf-8");
        (await response.Content.ReadAsStringAsync(Cancel)).ShouldBe("Welcome to Nicolas Qui Paie API.");
    }

    [Fact]
    public async Task TheSite_AnswersTheStandardReport_WithNothingToCheck_AndKeepsItsDemoAddress()
    {
        // Arrange
        await using var site = new HostApp();
        using var client = site.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Act
        using var health = await client.GetAsync("/health", Cancel);
        using var demo = await client.GetAsync("/demo", Cancel);

        // Assert
        health.StatusCode.ShouldBe(HttpStatusCode.OK);
        $"{health.Content.Headers.ContentType}".ShouldBe("application/json; charset=utf-8");
        health.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        var report = await ReadAsync(health);
        HealthShape.NamesOf(report).ShouldBe(HealthShape.Report);
        report.GetProperty("status").GetString().ShouldBe("Healthy");
        report.GetProperty("service").GetString().ShouldBe("Nicolas Qui Paie");
        report.GetProperty("checks").GetArrayLength().ShouldBe(0);
        demo.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        demo.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe("/?demo");
    }
}
