namespace NicolasQuiPaieWeb.Host.Extensions;

/// <summary>
/// What the host is made of: the front is a Blazor WebAssembly app, this host only hands out its files.
/// Any other address is a client route. Program.cs only puts the members together.
/// </summary>
public static class HostSetup
{
    extension(WebApplication app)
    {
        /// <summary>The files of the Blazor app, then the rest of wwwroot.</summary>
        public WebApplication UseFrontFiles()
        {
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            return app;
        }

        /// <summary>Presenter mode (/demo opens the home page under the demo's curtain), then index.html for every client route.</summary>
        public WebApplication MapFront()
        {
            app.MapDemo();
            app.MapFallbackToFile("index.html");
            return app;
        }
    }
}
