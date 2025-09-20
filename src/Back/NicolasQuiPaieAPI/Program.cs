using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configure Serilog with custom ApiLog table mapping
builder.AddCustomSerilog();

builder.Services.AddNicolasQuiPaieServices(builder.Configuration, builder.Environment);

// Add ASP.NET Core Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseNicolasQuiPaieMiddlewares(app.Configuration);

// 🎯 Initialize database using extension method
await app.InitializeDatabaseAsync();

Log.Information("🚀 Nicolas Qui Paie API started in {Environment} mode. Swagger: {SwaggerEnabled}. SQL Logging: Enabled with custom ApiLog table. 🔐 Role-based Authorization: Active",
    app.Environment.EnvironmentName, app.Environment.IsDevelopment() ? "Available" : "Disabled");

// Map /health endpoint to use Health Checks middleware
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = $"{report.Status}",
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

await app.RunAsync(); // Use async version

// Make Program class accessible for testing
public partial class Program { }