var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// The front is a Blazor WebAssembly app: this host only hands out its files. Any other address is a client route.
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok("Healthy"));
app.MapFallbackToFile("index.html");

app.Run();
