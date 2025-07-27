var builder = WebApplication.CreateBuilder(args);

// 🔧 Configure Serilog with custom ApiLog table mapping
builder.AddCustomSerilog();

builder.Services.AddNicolasQuiPaieServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseNicolasQuiPaieMiddlewares(app.Configuration);

// 🎯 Initialize database using extension method
await app.InitializeDatabaseAsync();

Log.Information("🚀 Nicolas Qui Paie API started in {Environment} mode. Swagger: {SwaggerEnabled}. SQL Logging: Enabled with custom ApiLog table. 🔐 Role-based Authorization: Active",
    app.Environment.EnvironmentName, app.Environment.IsDevelopment() ? "Available" : "Disabled");

await app.RunAsync(); // Use async version

// Make Program class accessible for testing
public partial class Program { }