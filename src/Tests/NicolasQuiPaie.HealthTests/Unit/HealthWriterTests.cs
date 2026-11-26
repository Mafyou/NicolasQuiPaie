namespace NicolasQuiPaie.HealthTests.Unit;

/// <summary>The one writer of the report, alone: its shape, the HTTP code of each status, and nothing an exception said.</summary>
public class HealthWriterTests
{
    private sealed record Written(int StatusCode, string? CacheControl, string? ContentType, string Text, JsonElement Body);

    private static async Task<Written> WriteAsync(Func<HttpContext, HealthReport, Task> writer, HealthReport report)
    {
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(host => host.EnvironmentName).Returns("Production");
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddSingleton(environment.Object).BuildServiceProvider()
        };
        context.Response.Body = new MemoryStream();

        await writer(context, report);

        context.Response.Body.Position = 0;
        var text = await new StreamReader(context.Response.Body).ReadToEndAsync(TestContext.Current.CancellationToken);
        using var json = JsonDocument.Parse(text);
        return new(context.Response.StatusCode, context.Response.Headers.CacheControl, context.Response.ContentType, text, json.RootElement.Clone());
    }

    private static HealthReport Report(HealthStatus status, string? description = null, Exception? exception = null) =>
        new(new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new(status, description, TimeSpan.FromMilliseconds(5), exception, null)
        }, TimeSpan.FromMilliseconds(12));

    [Fact]
    public async Task AHealthyApi_IsWrittenInTheStandardShape()
    {
        // Act
        var written = await WriteAsync(ApiHealth.WriteAsync, Report(HealthStatus.Healthy));

        // Assert
        written.StatusCode.ShouldBe(StatusCodes.Status200OK);
        written.CacheControl.ShouldBe("no-store");
        written.ContentType.ShouldBe("application/json; charset=utf-8");
        HealthShape.NamesOf(written.Body).ShouldBe(HealthShape.Report);
        written.Body.GetProperty("status").GetString().ShouldBe("Healthy");
        written.Body.GetProperty("service").GetString().ShouldBe("Nicolas Qui Paie API");
        written.Body.GetProperty("environment").GetString().ShouldBe("Production");
        written.Body.GetProperty("durationMs").GetInt64().ShouldBe(12);
        written.Body.GetProperty("timestamp").GetDateTime().ShouldBe(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var check = written.Body.GetProperty("checks").EnumerateArray().ShouldHaveSingleItem();
        HealthShape.NamesOf(check).ShouldBe(HealthShape.Check);
        check.GetProperty("name").GetString().ShouldBe("database");
        check.GetProperty("status").GetString().ShouldBe("Healthy");
        check.GetProperty("durationMs").GetInt64().ShouldBe(5);
    }

    [Theory]
    [InlineData(HealthStatus.Healthy, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Degraded, StatusCodes.Status200OK)]
    [InlineData(HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable)]
    public async Task EachStatus_HasItsHttpCode(HealthStatus status, int expected)
    {
        // Act
        var written = await WriteAsync(ApiHealth.WriteAsync, Report(status, "Fixed sentence."));

        // Assert
        written.StatusCode.ShouldBe(expected);
        written.Body.GetProperty("status").GetString().ShouldBe($"{status}");
    }

    [Fact]
    public async Task ACheckThatThrew_ReadsAsAFixedSentence_NeverAsItsException()
    {
        // Arrange: what the health checks service keeps of a check that threw
        var exception = new InvalidOperationException("Login failed for user 'sa' on secret-sql.database.windows.net, Password=hunter2");

        // Act
        var written = await WriteAsync(ApiHealth.WriteAsync, Report(HealthStatus.Unhealthy, exception.Message, exception));

        // Assert
        written.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
        written.Body.GetProperty("checks")[0].GetProperty("description").GetString().ShouldBe("Database unreachable.");
        written.Text.ShouldNotContain("secret-sql");
        written.Text.ShouldNotContain("hunter2");
        written.Text.ShouldNotContain("Login failed");
    }

    [Fact]
    public async Task TheVersion_LeavesOutTheGitHash()
    {
        // Act
        var written = await WriteAsync(ApiHealth.WriteAsync, Report(HealthStatus.Healthy));

        // Assert
        var version = written.Body.GetProperty("version").GetString();
        version.ShouldNotBeNullOrWhiteSpace();
        version.ShouldNotContain('+');
    }

    [Fact]
    public async Task TheSite_WithNothingToCheck_IsHealthy_WithAnEmptyListOfChecks()
    {
        // Act
        var written = await WriteAsync(HostHealth.WriteAsync, new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.Zero));

        // Assert
        written.StatusCode.ShouldBe(StatusCodes.Status200OK);
        HealthShape.NamesOf(written.Body).ShouldBe(HealthShape.Report);
        written.Body.GetProperty("service").GetString().ShouldBe("Nicolas Qui Paie");
        written.Body.GetProperty("status").GetString().ShouldBe("Healthy");
        written.Body.GetProperty("checks").GetArrayLength().ShouldBe(0);
    }
}
