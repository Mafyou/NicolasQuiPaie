extern alias Api;
extern alias WebHost;

using Api::NicolasQuiPaieAPI.Extensions;
using ApiProgram = Api::Program;
using WebHostProgram = WebHost::Program;

[assembly: AssemblyFixture(typeof(Platform))]
// One platform for the run, shared by journeys that each create their own users and proposals: one test at a time.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace NicolasQuiPaie.FunctionalTests.Support;

/// <summary>
/// The whole platform on this machine, as a visitor meets it: the API and the front's host each listening on a
/// loopback port of their own, the API on a SQLite database filled with the demo site's proposals, and one Chromium.
/// </summary>
public sealed class Platform : IAsyncLifetime
{
    private readonly ApiServer _api = new();
    private readonly FrontServer _front = new();
    private IPlaywright? _playwright;

    /// <summary>The API's address, e.g. http://127.0.0.1:51234/</summary>
    public Uri ApiAddress { get; private set; } = null!;

    /// <summary>The front host's address.</summary>
    public Uri FrontAddress { get; private set; } = null!;

    public IBrowser Browser { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        ApiAddress = _api.Start();
        await _api.SeedDemoDataAsync();
        FrontAddress = _front.Start();

        // The WebAssembly app downloads and starts before anything shows: the default five seconds are short.
        Assertions.SetDefaultExpectTimeout(30_000);
        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS")?.ToLowerInvariant() != "false"
        });
    }

    /// <summary>A plain HTTP client of the API: nothing of the app is shared with it but the address.</summary>
    public HttpClient CreateApiClient() => new() { BaseAddress = ApiAddress };

    /// <summary>A plain HTTP client of the front's host, which does not follow redirections.</summary>
    public HttpClient CreateFrontClient() => new(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = FrontAddress };

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();
        await _front.DisposeAsync();
        await _api.DisposeAsync();
    }

    /// <summary>Starts the factory's app on Kestrel, on a free loopback port, and gives its address.</summary>
    private static Uri StartOnLoopback<TEntryPoint>(WebApplicationFactory<TEntryPoint> factory) where TEntryPoint : class
    {
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        factory.StartServer();
        var addresses = factory.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses;
        return new Uri(addresses.First().TrimEnd('/') + "/");
    }

    /// <summary>The API as it is wired, on a SQLite file of its own, open to the front's loopback origin.</summary>
    private sealed class ApiServer : WebApplicationFactory<ApiProgram>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"nqp-functional-{Guid.NewGuid():N}.db");

        public Uri Start() => StartOnLoopback(this);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("DisableDbInitialization", "true");
            builder.UseSetting("DisableHttpsRedirection", "true");

            builder.ConfigureServices(services =>
            {
                var descriptorsToRemove = services.Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                    descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>)).ToList();
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={_databasePath}");
                    options.ReplaceService<IModelCustomizer, SqliteModelCustomizer>();
                });

                // The production policy lists the real origins: here the front answers on a loopback port.
                services.PostConfigure<CorsOptions>(options => options.AddPolicy("AllowBlazorClient", policy => policy
                    .SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));
            });
        }

        /// <summary>The demo site's sample proposals, by the API's own seeder, as the App Service deployment does.</summary>
        public async Task SeedDemoDataAsync()
        {
            await using var scope = Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            var settings = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> { [DemoDataSeeder.SettingKey] = "true" })
                .Build();
            await DemoDataSeeder.SeedAsync(settings, context, scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), NullLogger.Instance);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            SqliteConnection.ClearAllPools();
            File.Delete(_databasePath);
        }
    }

    /// <summary>NicolasQuiPaieWeb.Host as deployed: the Blazor files, /health, /demo and the client routes.</summary>
    private sealed class FrontServer : WebApplicationFactory<WebHostProgram>
    {
        public Uri Start() => StartOnLoopback(this);
    }
}
