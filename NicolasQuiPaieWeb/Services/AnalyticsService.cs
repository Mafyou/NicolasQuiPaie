using NicolasQuiPaieData.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Client-side service for analytics via API
/// </summary>
public class ApiAnalyticsService(HttpClient httpClient, ILogger<ApiAnalyticsService> logger)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<DashboardStatsDto>("/api/analytics/dashboard-stats", _jsonOptions);
            return response ?? new DashboardStatsDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving dashboard stats from API");
            return new DashboardStatsDto();
        }
    }

    public async Task<List<TrendingProposalDto>> GetTrendingProposalsAsync(int hours = 24)
    {
        try
        {
            // For now, get recent activity and convert to trending format
            var activity = await httpClient.GetFromJsonAsync<RecentActivityDto>("/api/analytics/recent-activity?take=10", _jsonOptions);
            
            // Convert activity to trending proposals (simplified)
            var trending = activity?.Activities?
                .Where(a => a.Type == "Proposal")
                .Select(a => new TrendingProposalDto
                {
                    Proposal = new ProposalDto 
                    { 
                        Id = int.TryParse(a.RelatedItemId, out var id) ? id : 0,
                        Title = a.RelatedItemTitle ?? "",
                        CreatedByDisplayName = a.UserDisplayName,
                        CategoryName = "General",
                        CategoryColor = "#007bff"
                    },
                    RecentVotes = 1,
                    RecentComments = 0,
                    TrendScore = 1
                })
                .ToList() ?? [];

            return trending;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving trending proposals from API");
            return [];
        }
    }

    public async Task<List<TopContributorDto>> GetTopContributorsAsync(int count = 10)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<TopContributorsDto>($"/api/analytics/top-contributors?take={count}", _jsonOptions);
            
            // Convert UserContributionDto to TopContributorDto
            var contributors = response?.TopProposers?.Take(count)
                .Select(u => new TopContributorDto
                {
                    UserId = u.UserId,
                    UserDisplayName = u.UserDisplayName,
                    UserFiscalLevel = u.UserFiscalLevel,
                    ProposalsCount = u.ContributionCount,
                    VotesCount = 0, // Will be loaded separately
                    CommentsCount = 0, // Will be loaded separately
                    TotalScore = u.ReputationScore
                })
                .ToList() ?? [];
            
            return contributors;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving top contributors from API");
            return [];
        }
    }
}

/// <summary>
/// Client-side wrapper service for analytics - now uses real API
/// </summary>
public class AnalyticsService(ApiAnalyticsService apiAnalyticsService)
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        => await apiAnalyticsService.GetDashboardStatsAsync();

    public async Task<List<TrendingProposalDto>> GetTrendingProposalsAsync(int hours = 24)
        => await apiAnalyticsService.GetTrendingProposalsAsync(hours);

    public async Task<List<TopContributorDto>> GetTopContributorsAsync(int count = 10)
        => await apiAnalyticsService.GetTopContributorsAsync(count);
}