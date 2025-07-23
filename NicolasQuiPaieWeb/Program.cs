var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API Base URL with fallback
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7051";

// Configure HttpClient for API calls with timeout and base configuration
builder.Services.AddScoped(sp => 
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
builder.Services.AddBlazoredLocalStorage();

// Configure Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Add API Services (direct HTTP clients) with error handling
builder.Services.AddScoped<ApiProposalService>();
builder.Services.AddScoped<ApiVotingService>();
builder.Services.AddScoped<ApiCommentService>();
builder.Services.AddScoped<ApiAnalyticsService>();

// Add Client-side wrapper services (for backward compatibility)
builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<VotingService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<BadgeService>();

// Add health check service for API availability
builder.Services.AddScoped<ApiHealthService>();

// Add logging with enhanced configuration
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
    logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
    logging.AddFilter("Microsoft.AspNetCore.Components.WebAssembly", LogLevel.Warning);
});

var app = builder.Build();

// Add global error handler
app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup")
    .LogInformation("Nicolas Qui Paie Web Application Starting - API Base URL: {ApiBaseUrl}", apiBaseUrl);

await app.RunAsync();
