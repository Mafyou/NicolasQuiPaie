namespace NicolasQuiPaie.UnitTests.Showroom;

/// <summary>The presenter mode's address: /demo leads to the home page under the curtain, ?demo.</summary>
public class DemoEndpointsTests
{
    [Fact]
    public async Task Demo_ShouldRedirectToTheHomePageWithTheDemoFlag()
    {
        // Arrange: a host with the front's demo endpoint only
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapDemo();
        await app.StartAsync(TestContext.Current.CancellationToken);
        using var client = app.GetTestClient();

        // Act
        using var response = await client.GetAsync("/demo", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"/?{DemoEndpoints.QueryKey}");
    }
}
