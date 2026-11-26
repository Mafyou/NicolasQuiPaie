// The API edits a static map of JwtSecurityTokenHandler as it starts: two hosts starting at once would race on it.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace NicolasQuiPaie.HealthTests.Support;

/// <summary>
/// The API as it is wired, but on a database of the test's own: an in-memory one that answers, or a SQL Server
/// address where nothing listens, as when the production database is down.
/// </summary>
public sealed class ApiApp(bool databaseDown = false) : WebApplicationFactory<ApiProgram>
{
    /// <summary>Port 9 of the loopback: the connection is refused at once, no host name leaves the machine.</summary>
    public const string UnreachableDatabase = "Server=tcp:127.0.0.1,9;Database=nqp;User ID=nobody;Password=nothing;Connect Timeout=1;Encrypt=False";

    private readonly string _databaseName = $"nqp-health-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("DisableDbInitialization", "true");
        builder.UseSetting("DisableHttpsRedirection", "true");

        builder.ConfigureServices(services =>
        {
            var database = services.Where(descriptor =>
                descriptor.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                descriptor.ServiceType == typeof(IDbContextOptionsConfiguration<ApplicationDbContext>)).ToList();
            foreach (var descriptor in database)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (databaseDown)
                {
                    options.UseSqlServer(UnreachableDatabase);
                }
                else
                {
                    options.UseInMemoryDatabase(_databaseName);
                }
            });
        });
    }
}

/// <summary>NicolasQuiPaieWeb.Host as deployed: the Blazor files, /health, /demo and the client routes.</summary>
public sealed class HostApp : WebApplicationFactory<WebHostProgram>;

/// <summary>An app of a factory on a real loopback port, as the health watch meets it over the network.</summary>
public static class Loopback
{
    public static Uri Start<TEntryPoint>(WebApplicationFactory<TEntryPoint> factory) where TEntryPoint : class
    {
        factory.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        factory.StartServer();
        var addresses = factory.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses;
        return new Uri($"{addresses.First().TrimEnd('/')}/");
    }
}

/// <summary>The names every report carries, in their order.</summary>
public static class HealthShape
{
    public static readonly string[] Report = ["status", "service", "version", "environment", "timestamp", "durationMs", "checks"];
    public static readonly string[] Check = ["name", "status", "durationMs", "description"];

    public static string[] NamesOf(JsonElement element) => [.. element.EnumerateObject().Select(property => property.Name)];
}
