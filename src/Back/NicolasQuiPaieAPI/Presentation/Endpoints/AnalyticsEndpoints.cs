namespace NicolasQuiPaieAPI.Presentation.Endpoints;

/// <summary>
/// Analytics endpoints with role-based authorization and modern language features
/// </summary>
public static class AnalyticsEndpoints
{
    /// <summary>
    /// Map analytics endpoints with role-based authorization
    /// </summary>
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .WithOpenApi()
            .RequireUserRole(); // Require User role or higher for all analytics

        // GET /api/analytics/global-stats - Basic analytics for all authenticated users
        group.MapGet("/global-stats", GetGlobalStatsAsync)
            .WithName("GetGlobalStats")
            .WithSummary("Récupère les statistiques globales")
            .Produces<GlobalStatsDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/voting-trends - Detailed trends for authenticated users
        group.MapGet("/voting-trends", GetVotingTrendsAsync)
            .WithName("GetVotingTrends")
            .WithSummary("Récupère les tendances de vote")
            .Produces<VotingTrendsDto>()
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/contribution-distribution - For all authenticated users
        group.MapGet("/contribution-distribution", GetContributionDistributionAsync)
            .WithName("GetContributionDistribution")
            .WithSummary("Récupère la distribution des niveaux de contribution")
            .Produces<ContributionLevelDistributionDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/top-contributors - Public leaderboard for authenticated users
        group.MapGet("/top-contributors", GetTopContributorsAsync)
            .WithName("GetTopContributors")
            .WithSummary("Récupère les meilleurs contributeurs")
            .Produces<TopContributorsDto>()
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/recent-activity - For all authenticated users
        group.MapGet("/recent-activity", GetRecentActivityAsync)
            .WithName("GetRecentActivity")
            .WithSummary("Récupère l'activité récente")
            .Produces<RecentActivityDto>()
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/frustration-barometer - Public mood indicator
        group.MapGet("/frustration-barometer", GetFrustrationBarometerAsync)
            .WithName("GetFrustrationBarometer")
            .WithSummary("Récupère le baromètre de frustration")
            .Produces<FrustrationBarometerDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/dashboard-stats - Enhanced dashboard for authenticated users
        group.MapGet("/dashboard-stats", GetDashboardStatsAsync)
            .WithName("GetDashboardStats")
            .WithSummary("Récupère les statistiques pour le tableau de bord")
            .Produces<DashboardStatsDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // Advanced analytics endpoints requiring SuperUser role
        MapAdvancedAnalyticsEndpoints(group);
    }

    /// <summary>
    /// Map advanced analytics endpoints for SuperUser and Admin roles
    /// </summary>
    private static void MapAdvancedAnalyticsEndpoints(RouteGroupBuilder group)
    {
        var advancedGroup = group.MapGroup("/advanced")
            .RequireSuperUserRole(); // Require SuperUser or Admin role

        // GET /api/analytics/advanced/user-engagement - Detailed user analytics
        advancedGroup.MapGet("/user-engagement", GetUserEngagementAsync)
            .WithName("GetUserEngagement")
            .WithSummary("Récupère les statistiques d'engagement utilisateur (SuperUser+)")
            .Produces<UserEngagementDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // GET /api/analytics/advanced/moderation-stats - Moderation statistics
        advancedGroup.MapGet("/moderation-stats", GetModerationStatsAsync)
            .WithName("GetModerationStats")
            .WithSummary("Récupère les statistiques de modération (SuperUser+)")
            .Produces<ModerationStatsDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);

        // Admin-only endpoints
        var adminGroup = advancedGroup.MapGroup("/admin")
            .RequireAdminRole(); // Require Admin role only

        // GET /api/analytics/advanced/admin/system-health - System health metrics
        adminGroup.MapGet("/system-health", GetSystemHealthAsync)
            .WithName("GetSystemHealth")
            .WithSummary("Récupère les métriques de santé système (Admin uniquement)")
            .Produces<SystemHealthDto>()
            .Produces(401)
            .Produces(403)
            .Produces(500);
    }

    #region Modern endpoint handlers with enhanced error handling

    /// <summary>
    /// Global stats handler with modern error handling
    /// </summary>
    private static async Task<IResult> GetGlobalStatsAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            var stats = await analyticsService.GetGlobalStatsAsync();

            // Modern null checking and pattern matching
            if (stats is { TotalUsers: 0, TotalProposals: 0 })
            {
                logger.LogWarning("Global stats request returned empty data - no users or proposals found");
            }

            // Log user access for analytics
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Global stats accessed by user {UserId}", userId);

            return Results.Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving global stats");
            return Results.Problem("Error retrieving global stats");
        }
    }

    /// <summary>
    /// Voting trends handler with parameter validation
    /// </summary>
    private static async Task<IResult> GetVotingTrendsAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user,
        [FromQuery] int days = 30)
    {
        try
        {
            // Pattern matching for validation
            if (days is <= 0 or > 365)
            {
                logger.LogWarning("Invalid days parameter for voting trends: {Days}. Must be between 1 and 365", days);
                return Results.BadRequest("Days parameter must be between 1 and 365");
            }

            var trends = await analyticsService.GetVotingTrendsAsync(days);

            // Modern collection checking
            if (trends.DailyVotes.Count == 0)
            {
                logger.LogWarning("No voting trends data found for the last {Days} days", days);
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Voting trends ({Days} days) accessed by user {UserId}", days, userId);

            return Results.Ok(trends);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving voting trends for {Days} days", days);
            return Results.Problem("Error retrieving voting trends");
        }
    }

    /// <summary>
    /// Contribution distribution handler
    /// </summary>
    private static async Task<IResult> GetContributionDistributionAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            var distribution = await analyticsService.GetContributionLevelDistributionAsync();

            // Modern null and collection checking
            if (distribution.Distribution.Count == 0)
            {
                logger.LogWarning("No contribution level distribution data found");
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Contribution distribution accessed by user {UserId}", userId);

            return Results.Ok(distribution);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving contribution level distribution");
            return Results.Problem("Error retrieving contribution level distribution");
        }
    }

    /// <summary>
    /// Top contributors handler with validation
    /// </summary>
    private static async Task<IResult> GetTopContributorsAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user,
        [FromQuery] int take = 10)
    {
        try
        {
            // Range pattern matching
            if (take is <= 0 or > 100)
            {
                logger.LogWarning("Invalid take parameter for top contributors: {Take}. Must be between 1 and 100", take);
                return Results.BadRequest("Take parameter must be between 1 and 100");
            }

            var contributors = await analyticsService.GetTopContributorsAsync(take);

            // Complex condition with modern syntax
            if (contributors is { TopProposers.Count: 0, TopVoters.Count: 0, TopCommenters.Count: 0 })
            {
                logger.LogWarning("No top contributors data found for take={Take}", take);
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Top contributors (take={Take}) accessed by user {UserId}", take, userId);

            return Results.Ok(contributors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving top contributors for take={Take}", take);
            return Results.Problem("Error retrieving top contributors");
        }
    }

    /// <summary>
    /// Recent activity handler
    /// </summary>
    private static async Task<IResult> GetRecentActivityAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user,
        [FromQuery] int take = 20)
    {
        try
        {
            // Range validation with pattern matching
            if (take is <= 0 or > 100)
            {
                logger.LogWarning("Invalid take parameter for recent activity: {Take}. Must be between 1 and 100", take);
                return Results.BadRequest("Take parameter must be between 1 and 100");
            }

            var activity = await analyticsService.GetRecentActivityAsync(take);

            if (activity.Activities.Count == 0)
            {
                logger.LogWarning("No recent activity data found for take={Take}", take);
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Recent activity (take={Take}) accessed by user {UserId}", take, userId);

            return Results.Ok(activity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving recent activity for take={Take}", take);
            return Results.Problem("Error retrieving recent activity");
        }
    }

    /// <summary>
    /// Frustration barometer handler with mood analysis
    /// </summary>
    private static async Task<IResult> GetFrustrationBarometerAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            var barometer = await analyticsService.GetFrustrationBarometerAsync();

            if (barometer.TotalVotes == 0)
            {
                logger.LogWarning("Frustration barometer request returned no votes data");
            }

            // Enhanced pattern matching for logging
            switch (barometer.FrustrationLevel)
            {
                case >= 80:
                    logger.LogWarning("High frustration level detected: {FrustrationLevel}% - Current mood: {CurrentMood}",
                        barometer.FrustrationLevel, barometer.CurrentMood);
                    break;
                case >= 60:
                    logger.LogInformation("Moderate frustration level: {FrustrationLevel}% - Current mood: {CurrentMood}",
                        barometer.FrustrationLevel, barometer.CurrentMood);
                    break;
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Frustration barometer accessed by user {UserId}", userId);

            return Results.Ok(barometer);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving frustration barometer");
            return Results.Problem("Error retrieving frustration barometer");
        }
    }

    /// <summary>
    /// Dashboard stats handler with comprehensive metrics
    /// </summary>
    private static async Task<IResult> GetDashboardStatsAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            var dashboardStats = await analyticsService.GetDashboardStatsAsync();

            if (dashboardStats.TotalUsers == 0)
            {
                logger.LogWarning("Dashboard stats request returned no user data");
            }

            // Complex condition checking with modern syntax
            if (dashboardStats is { ActiveUsers: 0, TotalUsers: > 0 })
            {
                logger.LogWarning("No active users detected despite having {TotalUsers} total users",
                    dashboardStats.TotalUsers);
            }

            if (dashboardStats.RasLebolMeter > 75)
            {
                logger.LogWarning("High 'Ras-le-bol' meter detected: {RasLebolMeter}%",
                    dashboardStats.RasLebolMeter);
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogInformation("Dashboard stats accessed by user {UserId}", userId);

            return Results.Ok(dashboardStats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dashboard stats");
            return Results.Problem("Error retrieving dashboard stats");
        }
    }

    #endregion

    #region Advanced analytics handlers for elevated roles

    /// <summary>
    /// User engagement analytics for SuperUser+ roles
    /// </summary>
    private static async Task<IResult> GetUserEngagementAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            // This would be implemented in your analytics service
            // For now, return a placeholder
            var engagement = new UserEngagementDto
            {
                TotalActiveUsers = 150,
                DailyActiveUsers = 45,
                WeeklyActiveUsers = 120,
                MonthlyActiveUsers = 150,
                AverageSessionDuration = TimeSpan.FromMinutes(25),
                BounceRate = 0.35
            };

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            logger.LogWarning("User engagement analytics accessed by {Role} user {UserId}", userRole, userId);

            return Results.Ok(engagement);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user engagement analytics");
            return Results.Problem("Error retrieving user engagement analytics");
        }
    }

    /// <summary>
    /// Moderation statistics for SuperUser+ roles
    /// </summary>
    private static async Task<IResult> GetModerationStatsAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            // This would be implemented in your analytics service
            var moderationStats = new ModerationStatsDto
            {
                PendingProposals = 12,
                FlaggedComments = 5,
                ReportedUsers = 2,
                ProcessedReports = 28,
                AverageModerationTime = TimeSpan.FromHours(4)
            };

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = user.FindFirstValue(ClaimTypes.Role);
            logger.LogWarning("Moderation statistics accessed by {Role} user {UserId}", userRole, userId);

            return Results.Ok(moderationStats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving moderation statistics");
            return Results.Problem("Error retrieving moderation statistics");
        }
    }

    /// <summary>
    /// System health metrics for Admin role only
    /// </summary>
    private static async Task<IResult> GetSystemHealthAsync(
        [FromServices] IAnalyticsService analyticsService,
        [FromServices] ILogger<Program> logger,
        ClaimsPrincipal user)
    {
        try
        {
            // This would be implemented in your analytics service
            var systemHealth = new SystemHealthDto
            {
                DatabaseHealth = "Healthy",
                ApiResponseTime = TimeSpan.FromMilliseconds(150),
                MemoryUsage = 0.65,
                CpuUsage = 0.35,
                ActiveConnections = 45,
                ErrorRate = 0.02
            };

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            logger.LogWarning("System health metrics accessed by Admin user {UserId}", userId);

            return Results.Ok(systemHealth);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system health metrics");
            return Results.Problem("Error retrieving system health metrics");
        }
    }

    #endregion
}