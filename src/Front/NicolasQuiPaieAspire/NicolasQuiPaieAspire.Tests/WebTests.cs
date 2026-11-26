namespace NicolasQuiPaieAspire.Tests;

/// <summary>
/// The whole Aspire application (API and front) started by its AppHost. It needs the Aspire orchestrator and the API's
/// local SQL Server, which neither CI nor every machine has: opt in with NQP_ASPIRE_TESTS=1.
/// </summary>
public class WebTests
{
    public const string OptInVariable = "NQP_ASPIRE_TESTS";

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        Assert.SkipUnless(Environment.GetEnvironmentVariable(OptInVariable) == "1",
            $"Set {OptInVariable}=1 to start the Aspire AppHost (orchestrator and local SQL Server needed).");

        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.NicolasQuiPaieAspire_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        // Act
        var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        var response = await httpClient.GetAsync("/", cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
