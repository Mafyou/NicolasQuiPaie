namespace NicolasQuiPaieAPI.Presentation.Endpoints;

public static class AnalyticsEndpoints
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .WithTags("Analytics")
            .WithOpenApi();

        // GET /api/analytics/global-stats
        group.MapGet("/global-stats", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var stats = await analyticsService.GetGlobalStatsAsync();
                
                if (stats.TotalUsers == 0 && stats.TotalProposals == 0)
                {
                    logger.LogWarning("Global stats request returned empty data - no users or proposals found");
                }

                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving global stats");
                return Results.Problem("Error retrieving global stats");
            }
        })
        .WithName("GetGlobalStats")
        .WithSummary("Récupère les statistiques globales")
        .Produces<GlobalStatsDto>()
        .Produces(500);

        // GET /api/analytics/voting-trends
        group.MapGet("/voting-trends", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int days = 30) =>
        {
            try
            {
                if (days <= 0 || days > 365)
                {
                    logger.LogWarning("Invalid days parameter for voting trends: {Days}. Must be between 1 and 365", days);
                    return Results.BadRequest("Days parameter must be between 1 and 365");
                }

                var trends = await analyticsService.GetVotingTrendsAsync(days);
                
                if (trends.DailyVotes.Count == 0)
                {
                    logger.LogWarning("No voting trends data found for the last {Days} days", days);
                }

                return Results.Ok(trends);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving voting trends for {Days} days", days);
                return Results.Problem("Error retrieving voting trends");
            }
        })
        .WithName("GetVotingTrends")
        .WithSummary("Récupère les tendances de vote")
        .Produces<VotingTrendsDto>()
        .Produces(400)
        .Produces(500);

        // GET /api/analytics/contribution-distribution
        group.MapGet("/contribution-distribution", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var distribution = await analyticsService.GetContributionLevelDistributionAsync();
                
                if (distribution.Distribution.Count == 0)
                {
                    logger.LogWarning("No contribution level distribution data found");
                }

                return Results.Ok(distribution);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving contribution level distribution");
                return Results.Problem("Error retrieving contribution level distribution");
            }
        })
        .WithName("GetContributionDistribution")
        .WithSummary("Récupère la distribution des niveaux de contribution")
        .Produces<ContributionLevelDistributionDto>()
        .Produces(500);

        // GET /api/analytics/top-contributors
        group.MapGet("/top-contributors", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int take = 10) =>
        {
            try
            {
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter for top contributors: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                var contributors = await analyticsService.GetTopContributorsAsync(take);
                
                if (contributors.TopProposers.Count == 0 && contributors.TopVoters.Count == 0 && contributors.TopCommenters.Count == 0)
                {
                    logger.LogWarning("No top contributors data found for take={Take}", take);
                }

                return Results.Ok(contributors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving top contributors for take={Take}", take);
                return Results.Problem("Error retrieving top contributors");
            }
        })
        .WithName("GetTopContributors")
        .WithSummary("Récupère les meilleurs contributeurs")
        .Produces<TopContributorsDto>()
        .Produces(400)
        .Produces(500);

        // GET /api/analytics/recent-activity
        group.MapGet("/recent-activity", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger,
            [FromQuery] int take = 20) =>
        {
            try
            {
                if (take <= 0 || take > 100)
                {
                    logger.LogWarning("Invalid take parameter for recent activity: {Take}. Must be between 1 and 100", take);
                    return Results.BadRequest("Take parameter must be between 1 and 100");
                }

                var activity = await analyticsService.GetRecentActivityAsync(take);
                
                if (activity.Activities.Count == 0)
                {
                    logger.LogWarning("No recent activity data found for take={Take}", take);
                }

                return Results.Ok(activity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving recent activity for take={Take}", take);
                return Results.Problem("Error retrieving recent activity");
            }
        })
        .WithName("GetRecentActivity")
        .WithSummary("Récupère l'activité récente")
        .Produces<RecentActivityDto>()
        .Produces(400)
        .Produces(500);

        // GET /api/analytics/frustration-barometer
        group.MapGet("/frustration-barometer", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var barometer = await analyticsService.GetFrustrationBarometerAsync();
                
                if (barometer.TotalVotes == 0)
                {
                    logger.LogWarning("Frustration barometer request returned no votes data");
                }

                // Log high frustration levels as warnings
                if (barometer.FrustrationLevel > 80)
                {
                    logger.LogWarning("High frustration level detected: {FrustrationLevel}% - Current mood: {CurrentMood}", 
                        barometer.FrustrationLevel, barometer.CurrentMood);
                }

                return Results.Ok(barometer);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving frustration barometer");
                return Results.Problem("Error retrieving frustration barometer");
            }
        })
        .WithName("GetFrustrationBarometer")
        .WithSummary("Récupère le baromètre de frustration")
        .Produces<FrustrationBarometerDto>()
        .Produces(500);

        // GET /api/analytics/dashboard-stats
        group.MapGet("/dashboard-stats", async (
            [FromServices] IAnalyticsService analyticsService,
            [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var dashboardStats = await analyticsService.GetDashboardStatsAsync();
                
                if (dashboardStats.TotalUsers == 0)
                {
                    logger.LogWarning("Dashboard stats request returned no user data");
                }

                // Log concerning metrics
                if (dashboardStats.ActiveUsers == 0 && dashboardStats.TotalUsers > 0)
                {
                    logger.LogWarning("No active users detected despite having {TotalUsers} total users", 
                        dashboardStats.TotalUsers);
                }

                if (dashboardStats.RasLebolMeter > 75)
                {
                    logger.LogWarning("High 'Ras-le-bol' meter detected: {RasLebolMeter}%", 
                        dashboardStats.RasLebolMeter);
                }

                return Results.Ok(dashboardStats);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving dashboard stats");
                return Results.Problem("Error retrieving dashboard stats");
            }
        })
        .WithName("GetDashboardStats")
        .WithSummary("Récupère les statistiques pour le tableau de bord")
        .Produces<DashboardStatsDto>()
        .Produces(500);
    }
}