var builder = WebApplication.CreateBuilder(args);

builder.AddCustomSerilog();

builder.Services
    .AddApiDocumentation(builder.Environment)
    .AddClientCors(builder.Environment)
    .AddDatabase(builder.Configuration)
    .AddSecurity(builder.Configuration)
    .AddPlatform()
    .AddMafyouHealth();

var app = builder.Build();

app.UseApiPipeline()
    .MapApiEndpoints()
    .MapMafyouHealth();

await app.InitializeDatabaseAsync();
app.AnnounceStart();

await app.RunAsync();

// Make Program class accessible for testing
public partial class Program { }
