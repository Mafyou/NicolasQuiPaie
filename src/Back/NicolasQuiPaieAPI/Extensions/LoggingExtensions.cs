namespace NicolasQuiPaieAPI.Extensions;

/// <summary>
/// Extension methods for configuring Serilog with custom ApiLog table structure
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog with SQL Server sink using existing ApiLog table structure
    /// </summary>
    public static WebApplicationBuilder AddCustomSerilog(this WebApplicationBuilder builder)
    {
        // Configuration Serilog with custom column mapping for existing ApiLog table
        var columnOptions = new ColumnOptions
        {
            // Disable default triggers and clustered columnstore index
            DisableTriggers = true,
            ClusteredColumnstoreIndex = false
        };

        // Remove default columns that we'll customize
        columnOptions.Store.Remove(StandardColumn.Properties);
        columnOptions.Store.Remove(StandardColumn.MessageTemplate);

        // Add custom columns to match your ApiLog entity structure
        columnOptions.AdditionalColumns =
        [
            new() { ColumnName = "UserId", DataType = SqlDbType.NVarChar, DataLength = 450, AllowNull = true },
            new() { ColumnName = "UserName", DataType = SqlDbType.NVarChar, DataLength = 256, AllowNull = true },
            new() { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 2048, AllowNull = true },
            new() { ColumnName = "RequestMethod", DataType = SqlDbType.NVarChar, DataLength = 10, AllowNull = true },
            new() { ColumnName = "StatusCode", DataType = SqlDbType.Int, AllowNull = true },
            new() { ColumnName = "Duration", DataType = SqlDbType.BigInt, AllowNull = true },
            new() { ColumnName = "ClientIP", DataType = SqlDbType.NVarChar, DataLength = 45, AllowNull = true },
            new() { ColumnName = "UserAgent", DataType = SqlDbType.NVarChar, DataLength = 1000, AllowNull = true },
            new() { ColumnName = "Source", DataType = SqlDbType.NVarChar, DataLength = 100, AllowNull = true },
            new() { ColumnName = "Properties", DataType = SqlDbType.NVarChar, DataLength = -1, AllowNull = true },
            new() { ColumnName = "MessageTemplate", DataType = SqlDbType.NVarChar, DataLength = 2000, AllowNull = true }
        ];

        // Customize standard columns to match your ApiLog entity
        columnOptions.Id.ColumnName = "Id";
        columnOptions.Level.ColumnName = "Level";
        columnOptions.Level.StoreAsEnum = true; // Store as int to match NicolasQuiPaieAPI.Infrastructure.Models.LogLevel enum
        columnOptions.Message.ColumnName = "Message";
        columnOptions.Message.DataLength = 4000; // Match your entity constraint
        columnOptions.TimeStamp.ColumnName = "TimeStamp";
        columnOptions.Exception.ColumnName = "Exception";

        // Configuration Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning() // Only capture Warning, Error, and Fatal
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console() // Add console logging for debugging
            .WriteTo.MSSqlServer(
                connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "ApiLogs",
                    SchemaName = "dbo",
                    AutoCreateSqlTable = false // Don't auto-create since table exists via migrations
                },
                columnOptions: columnOptions,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
            .Enrich.WithProperty("Application", "NicolasQuiPaieAPI")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
            .CreateLogger();

        // Verify Serilog configuration
        try
        {
            Log.Warning("✅ Serilog SQL Server configuration test - this should appear in ApiLogs table");
            Console.WriteLine("✅ Serilog configured successfully with custom ApiLog table mapping");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Serilog configuration error: {ex.Message}");
            throw;
        }

        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Adds request logging middleware to enrich Serilog logs with custom properties
    /// </summary>
    public static WebApplication UseCustomSerilogRequestLogging(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            using (Serilog.Context.LogContext.PushProperty("UserId", context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("id")?.Value))
            using (Serilog.Context.LogContext.PushProperty("UserName", context.User?.FindFirst("name")?.Value ?? context.User?.Identity?.Name))
            using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path.Value))
            using (Serilog.Context.LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (Serilog.Context.LogContext.PushProperty("ClientIP", context.Connection.RemoteIpAddress?.ToString()))
            using (Serilog.Context.LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
            using (Serilog.Context.LogContext.PushProperty("Source", "NicolasQuiPaieAPI"))
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                await next();

                stopwatch.Stop();

                using (Serilog.Context.LogContext.PushProperty("StatusCode", context.Response.StatusCode))
                using (Serilog.Context.LogContext.PushProperty("Duration", stopwatch.ElapsedMilliseconds))
                {
                    // Only log errors and warnings based on status code
                    if (context.Response.StatusCode >= 400)
                    {
                        if (context.Response.StatusCode >= 500)
                        {
                            Log.Error("Request completed with server error: {RequestMethod} {RequestPath} - {StatusCode} in {Duration}ms",
                                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                        }
                        else
                        {
                            Log.Warning("Request completed with client error: {RequestMethod} {RequestPath} - {StatusCode} in {Duration}ms",
                                context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
                        }
                    }
                }
            }
        });

        return app;
    }
}