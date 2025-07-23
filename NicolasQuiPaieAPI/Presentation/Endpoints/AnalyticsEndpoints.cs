using Microsoft.AspNetCore.Mvc;
using NicolasQuiPaieAPI.Application.Interfaces;
using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieAPI.Presentation.Endpoints
{
    public static class AnalyticsEndpoints
    {
        public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/analytics")
                .WithTags("Analytics")
                .WithOpenApi();

            // GET /api/analytics/global-stats
            group.MapGet("/global-stats", async ([FromServices] IAnalyticsService analyticsService) =>
            {
                try
                {
                    var stats = await analyticsService.GetGlobalStatsAsync();
                    return Results.Ok(stats);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving global stats: {ex.Message}");
                }
            })
            .WithName("GetGlobalStats")
            .WithSummary("Récupère les statistiques globales")
            .Produces<GlobalStatsDto>();

            // GET /api/analytics/voting-trends
            group.MapGet("/voting-trends", async (
                [FromServices] IAnalyticsService analyticsService,
                [FromQuery] int days = 30) =>
            {
                try
                {
                    var trends = await analyticsService.GetVotingTrendsAsync(days);
                    return Results.Ok(trends);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving voting trends: {ex.Message}");
                }
            })
            .WithName("GetVotingTrends")
            .WithSummary("Récupère les tendances de vote")
            .Produces<VotingTrendsDto>();

            // GET /api/analytics/contribution-distribution
            group.MapGet("/contribution-distribution", async ([FromServices] IAnalyticsService analyticsService) =>
            {
                try
                {
                    var distribution = await analyticsService.GetContributionLevelDistributionAsync();
                    return Results.Ok(distribution);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving contribution level distribution: {ex.Message}");
                }
            })
            .WithName("GetContributionDistribution")
            .WithSummary("Récupère la distribution des niveaux de contribution")
            .Produces<ContributionLevelDistributionDto>();

            // GET /api/analytics/top-contributors
            group.MapGet("/top-contributors", async (
                [FromServices] IAnalyticsService analyticsService,
                [FromQuery] int take = 10) =>
            {
                try
                {
                    var contributors = await analyticsService.GetTopContributorsAsync(take);
                    return Results.Ok(contributors);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving top contributors: {ex.Message}");
                }
            })
            .WithName("GetTopContributors")
            .WithSummary("Récupère les meilleurs contributeurs")
            .Produces<TopContributorsDto>();

            // GET /api/analytics/recent-activity
            group.MapGet("/recent-activity", async (
                [FromServices] IAnalyticsService analyticsService,
                [FromQuery] int take = 20) =>
            {
                try
                {
                    var activity = await analyticsService.GetRecentActivityAsync(take);
                    return Results.Ok(activity);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving recent activity: {ex.Message}");
                }
            })
            .WithName("GetRecentActivity")
            .WithSummary("Récupère l'activité récente")
            .Produces<RecentActivityDto>();

            // GET /api/analytics/frustration-barometer
            group.MapGet("/frustration-barometer", async ([FromServices] IAnalyticsService analyticsService) =>
            {
                try
                {
                    var barometer = await analyticsService.GetFrustrationBarometerAsync();
                    return Results.Ok(barometer);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving frustration barometer: {ex.Message}");
                }
            })
            .WithName("GetFrustrationBarometer")
            .WithSummary("Récupère le baromètre de frustration")
            .Produces<FrustrationBarometerDto>();

            // GET /api/analytics/dashboard-stats
            group.MapGet("/dashboard-stats", async ([FromServices] IAnalyticsService analyticsService) =>
            {
                try
                {
                    var dashboardStats = await analyticsService.GetDashboardStatsAsync();
                    return Results.Ok(dashboardStats);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving dashboard stats: {ex.Message}");
                }
            })
            .WithName("GetDashboardStats")
            .WithSummary("Récupère les statistiques pour le tableau de bord")
            .Produces<DashboardStatsDto>();
        }
    }
}