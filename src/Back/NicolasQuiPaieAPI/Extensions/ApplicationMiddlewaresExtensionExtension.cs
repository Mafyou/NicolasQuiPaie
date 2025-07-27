namespace NicolasQuiPaieAPI.Extensions;

public static class ApplicationMiddlewaresExtensionExtension
{
    /// <summary>
    /// Adds the necessary middlewares for the NicolasQuiPaieAPI application
    /// </summary>
    public static void UseNicolasQuiPaieMiddlewares(this WebApplication app, IConfiguration configuration)
    {
        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            // Swagger ONLY available in Development mode
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nicolas Qui Paie API v1 - Development");
                c.RoutePrefix = string.Empty; // Swagger UI à la racine
                c.DocumentTitle = "Nicolas Qui Paie API - Development Mode";
            });

            // Use development CORS in development
            app.UseCors("DevelopmentCors");
        }
        else
        {
            // Use restrictive CORS in production
            app.UseCors("AllowBlazorClient");
        }

        // Only use HTTPS redirection if not explicitly disabled
        var disableHttpsRedirection = configuration.GetValue<bool>("DisableHttpsRedirection");
        if (!disableHttpsRedirection)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        // 🔧 Add custom Serilog request logging middleware
        app.UseCustomSerilogRequestLogging();

        // 🎯 Map all API endpoints using extension method
        app.MapApiEndpoints();

        // Default root endpoint and swagger redirects
        app.MapGet("/", () =>
        {
            if (app.Environment.IsDevelopment())
            {
                return Results.Content("""
        <!DOCTYPE html>
        <html>
        <head>
            <title>Nicolas Qui Paie API - Development</title>
            <meta http-equiv="refresh" content="0; url=/">
        </head>
        <body>
            <h1>Welcome to Nicolas Qui Paie API - Development Mode</h1>
            <p>Redirecting to Swagger documentation...</p>
            <p>If you are not redirected automatically, <a href="/">click here</a>.</p>
            <hr>
            <p><strong>Available endpoints:</strong></p>
            <ul>
                <li><a href="/swagger">Swagger Documentation</a></li>
                <li><a href="/health">Health Check</a></li>
                <li><a href="/api/logs">API Logs</a></li>
                <li><a href="/swagger/index.html">Test Logging (POST)</a></li>
            </ul>
            <p><em>Logging Level: Warning, Error, Fatal only</em></p>
            <p><em>SQL Logging: Enabled with custom ApiLog table mapping</em></p>
            <p><em>🔐 Role-based Authorization: User | SuperUser | Admin</em></p>
        </body>
        </html>
        """, "text/html");
            }
            else
            {
                return Results.Ok(new
                {
                    Status = "Nicolas Qui Paie API",
                    Environment = "Production",
                    Timestamp = DateTime.UtcNow,
                    Message = "API is running. Swagger documentation is only available in development mode.",
                    LoggingLevel = "Warning, Error, Fatal only",
                    SqlLoggingEnabled = true,
                    Authorization = "Role-based: User | SuperUser | Admin"
                });
            }
        })
        .WithTags("Root")
        .WithSummary("API root endpoint");

        // Handle swagger redirects only in development
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/swagger", () => Results.Redirect("/"))
               .WithTags("Root")
               .WithSummary("Redirect Swagger to root");

            app.MapGet("/Swagger", () => Results.Redirect("/"))
               .WithTags("Root")
               .WithSummary("Redirect Swagger (capital S) to root");
        }
    }
}