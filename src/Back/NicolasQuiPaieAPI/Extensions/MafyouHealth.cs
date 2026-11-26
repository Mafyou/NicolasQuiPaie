namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// "/" and "/health" as every server of Mafyou IT answers them: the health watch reads the report every 15 minutes.
/// Anonymous, never rate-limited, never cached; nothing in it says what failed or where.
/// </summary>
public static class MafyouHealth
{
    public const string Service = "Nicolas Qui Paie API";
    public const string Path = "/health";

    /// <summary>The longest the watch waits for one check.</summary>
    public static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(2);

    private static readonly string Version = typeof(MafyouHealth).Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0] ?? "1.0.0";

    extension(IServiceCollection services)
    {
        /// <summary>The one dependency of the API: its database.</summary>
        public IServiceCollection AddMafyouHealth()
        {
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");
            return services.Configure<HealthCheckServiceOptions>(options =>
            {
                foreach (var check in options.Registrations)
                {
                    check.Timeout = CheckTimeout;
                }
            });
        }
    }

    extension(WebApplication app)
    {
        public WebApplication MapMafyouHealth()
        {
            app.MapGet("/", () => Results.Text($"Welcome to {Service}.", "text/plain; charset=utf-8"))
                .AllowAnonymous()
                .DisableRateLimiting()
                .ExcludeFromDescription();
            app.MapHealthChecks(Path, new HealthCheckOptions
            {
                ResponseWriter = WriteAsync,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            })
                .AllowAnonymous()
                .DisableRateLimiting();
            return app;
        }
    }

    /// <summary>The one writer of the report: a check that threw or timed out reads as a fixed sentence, never as its exception.</summary>
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.StatusCode = report.Status is HealthStatus.Unhealthy ? StatusCodes.Status503ServiceUnavailable : StatusCodes.Status200OK;
        context.Response.Headers.CacheControl = "no-store";
        return context.Response.WriteAsJsonAsync(new
        {
            status = $"{report.Status}",
            service = Service,
            version = Version,
            environment = context.RequestServices.GetRequiredService<IHostEnvironment>().EnvironmentName,
            timestamp = DateTime.UtcNow,
            durationMs = (long)report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = $"{entry.Value.Status}",
                durationMs = (long)entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Exception is null && (entry.Value.Description is not null || entry.Value.Status is HealthStatus.Healthy)
                    ? entry.Value.Description
                    : $"{char.ToUpperInvariant(entry.Key[0])}{entry.Key[1..]} unreachable."
            })
        }, JsonSerializerOptions.Web, "application/json; charset=utf-8", context.RequestAborted);
    }
}
