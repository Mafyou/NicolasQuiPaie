var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Services
builder.Services.AddNicolasQuiPaieWebServices(builder.Configuration);

var app = builder.Build();

// Add global error handler
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7051";
app.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup")
    .LogInformation("Nicolas Qui Paie Web Application Starting - API Base URL: {ApiBaseUrl}", apiBaseUrl);

await app.RunAsync();