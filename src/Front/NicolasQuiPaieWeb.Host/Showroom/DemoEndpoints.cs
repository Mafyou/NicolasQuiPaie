namespace NicolasQuiPaieWeb.Host.Showroom;

/// <summary>
/// Presenter mode: <c>/demo</c>, the address to type in front of the audience, opens the home page under the demo's
/// curtain (<c>wwwroot/js/demo.js</c> of the front). A temporary redirect: the address must not be remembered.
/// </summary>
public static class DemoEndpoints
{
    public const string QueryKey = "demo";

    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointConventionBuilder MapDemo() =>
            endpoints.MapGet("/demo", (HttpContext context) => Results.Redirect($"{context.Request.PathBase}/?{QueryKey}"));
    }
}
