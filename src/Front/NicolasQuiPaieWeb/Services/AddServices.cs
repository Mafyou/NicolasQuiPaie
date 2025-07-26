namespace NicolasQuiPaieWeb.Services;

public static class AddServices
{
    public static IServiceCollection AddNicolasQuiPaieWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure API Base URL with fallback
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7051";

        // Configure maintenance settings
        services.Configure<MaintenanceSettings>(
            configuration.GetSection(MaintenanceSettings.SectionName));

        // Configure HttpClient for API calls with timeout and base configuration
        services.AddScoped(sp =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Add default headers
            httpClient.DefaultRequestHeaders.Add("User-Agent", "NicolasQuiPaieWeb/1.0");

            return httpClient;
        });

        // Add Blazored LocalStorage for JWT token storage
        services.AddBlazoredLocalStorage();

        // Configure Authentication
        services.AddAuthorizationCore();
        services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Add Sample Data Service for read-only mode
        services.AddScoped<SampleDataService>();

        // Add API Services (direct HTTP clients) with error handling
        services.AddScoped<ApiProposalService>();
        services.AddScoped<ApiVotingService>();
        services.AddScoped<ApiCommentService>();
        services.AddScoped<ApiAnalyticsService>();
        services.AddScoped<ApiLogsService>(); // Added new logs service

        // Add Client-side wrapper services (for backward compatibility)
        services.AddScoped<ProposalService>();
        services.AddScoped<VotingService>();
        services.AddScoped<CommentService>();
        services.AddScoped<AnalyticsService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<BadgeService>();

        // Add health check service for API availability
        services.AddScoped<ApiHealthService>();

        // Add logging with enhanced configuration
        services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            logging.AddFilter("System.Net.Http.HttpClient", Microsoft.Extensions.Logging.LogLevel.Warning);
            logging.AddFilter("Microsoft.AspNetCore.Components.WebAssembly", Microsoft.Extensions.Logging.LogLevel.Warning);
        });
        return services;
    }
}