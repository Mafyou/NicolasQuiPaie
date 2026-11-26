namespace NicolasQuiPaie.UnitTests.Showroom;

/// <summary>The presenter mode's address: /demo leads to the home page under the curtain, ?demo.</summary>
[TestFixture]
public class DemoEndpointsTests
{
    [Test]
    public async Task Demo_ShouldRedirectToTheHomePageWithTheDemoFlag()
    {
        // Arrange: a host with the front's demo endpoint only
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.MapDemo();
        await app.StartAsync();
        using var client = app.GetTestClient();

        // Act
        using var response = await client.GetAsync("/demo");

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe($"/?{DemoEndpoints.QueryKey}");
    }
}
