using Microsoft.AspNetCore.Mvc;
using NicolasQuiPaieAPI.Application.Interfaces;

namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Extension methods for API endpoint configuration
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all API endpoints
    /// </summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        // Map all endpoint groups
        app.MapAuthenticationEndpoints();
        app.MapProposalEndpoints();
        app.MapVotingEndpoints();
        app.MapCategoryEndpoints();
        app.MapCommentEndpoints();
        app.MapAnalyticsEndpoints();

        // Map monitoring and testing endpoints
        app.MapHealthEndpoint();
        app.MapLogsEndpoint();
        app.MapTestLoggingEndpoint();

        return app;
    }

    private static void MapHealthEndpoint(this WebApplication app)
    {
        app.MapGet("/health", (ILogger<Program> logger) =>
        {
            try
            {
                logger.LogWarning("Health check endpoint accessed - testing SQL logging with custom ApiLog table");
                
                return Results.Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Environment = app.Environment.EnvironmentName,
                    SwaggerAvailable = app.Environment.IsDevelopment(),
                    LoggingLevel = "Warning, Error, Fatal only",
                    SqlLoggingEnabled = true,
                    CustomApiLogTable = true
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error in health check endpoint");
                return Results.Problem("Health check failed");
            }
        })
        .WithTags("Health")
        .WithSummary("Vérification de l'état de l'API");
    }

    private static void MapLogsEndpoint(this WebApplication app)
    {
        app.MapGet("/api/logs", async (
            [FromServices] IApiLogRepository apiLogRepository,
            [FromServices] ILogger<Program> logger,
            [FromQuery] string? level = null,
            [FromQuery] int take = 100) =>
        {
            try
            {
                // Validate take parameter
                if (take <= 0 || take > 1000)
                {
                    logger.LogWarning("Invalid take parameter: {Take}. Must be between 1 and 1000", take);
                    return Results.BadRequest("Take parameter must be between 1 and 1000");
                }

                // Parse and validate level parameter
                NicolasQuiPaieAPI.Infrastructure.Models.LogLevel? logLevel = null;
                if (!string.IsNullOrEmpty(level))
                {
                    if (!Enum.TryParse<NicolasQuiPaieAPI.Infrastructure.Models.LogLevel>(level, true, out var parsedLevel))
                    {
                        var validLevels = string.Join(", ", Enum.GetNames<NicolasQuiPaieAPI.Infrastructure.Models.LogLevel>());
                        logger.LogWarning("Invalid level parameter: {Level}", level);
                        return Results.BadRequest($"Invalid level parameter. Valid values: {validLevels}");
                    }
                    logLevel = parsedLevel;
                }

                // Get logs using Entity Framework
                var logs = await apiLogRepository.GetLatestLogsAsync(take, logLevel);

                // Convert to response format
                var logDtos = logs.Select(log => new
                {
                    Id = log.Id,
                    Message = log.Message,
                    Level = log.Level.ToString(),
                    TimeStamp = log.TimeStamp,
                    Exception = log.Exception,
                    UserId = log.UserId,
                    UserName = log.UserName,
                    RequestPath = log.RequestPath,
                    RequestMethod = log.RequestMethod,
                    StatusCode = log.StatusCode,
                    Duration = log.Duration,
                    ClientIP = log.ClientIP,
                    Source = log.Source
                }).ToList();

                if (logDtos.Count == 0)
                {
                    logger.LogWarning("No logs found for filter: {Level}, take: {Take}", level ?? "All", take);
                }

                return Results.Ok(new
                {
                    TotalReturned = logDtos.Count,
                    LevelFilter = level,
                    RequestedCount = take,
                    AvailableLevels = Enum.GetNames<NicolasQuiPaieAPI.Infrastructure.Models.LogLevel>(),
                    Logs = logDtos
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error retrieving logs with level: {Level}, take: {Take}", level, take);
                return Results.Problem($"Error retrieving logs: {ex.Message}");
            }
        })
        .WithTags("Monitoring")
        .WithName("GetApiLogs")
        .WithSummary("Récupère les derniers logs de l'API filtrés par niveau")
        .Produces(200)
        .Produces(400)
        .Produces(500);
    }

    private static void MapTestLoggingEndpoint(this WebApplication app)
    {
        app.MapPost("/api/test-logging", (
            [FromServices] ILogger<Program> logger,
            [FromBody] TestLogRequest? request) =>
        {
            var message = request?.Message ?? "Test log entry";
            var level = request?.Level ?? "Warning";

            try
            {
                switch (level.ToLower())
                {
                    case "warning":
                        logger.LogWarning("Test Warning Log: {Message}", message);
                        break;
                    case "error":
                        logger.LogError("Test Error Log: {Message}", message);
                        break;
                    case "fatal":
                        logger.LogCritical("Test Fatal Log: {Message}", message);
                        break;
                    default:
                        logger.LogWarning("Test Log with invalid level ({Level}): {Message}", level, message);
                        break;
                }

                return Results.Ok(new
                {
                    Success = true,
                    Message = $"Test log entry created with level {level}",
                    LogMessage = message,
                    Timestamp = DateTime.UtcNow,
                    Note = "Check your ApiLogs table in the database to see the entry"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating test log entry");
                return Results.Problem("Error creating test log entry");
            }
        })
        .WithTags("Testing")
        .WithName("TestLogging")
        .WithSummary("Crée une entrée de log de test")
        .Produces(200)
        .Produces(500);
    }
}

/// <summary>
/// Record for test logging request
/// </summary>
public record TestLogRequest(string Message, string Level);