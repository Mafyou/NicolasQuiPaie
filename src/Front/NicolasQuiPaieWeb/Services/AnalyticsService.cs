namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Interface for analytics services
/// </summary>
public interface IAnalyticsService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<List<TrendingProposalDto>> GetTrendingProposalsAsync(int hours = 24);
    Task<List<TopContributorDto>> GetTopContributorsAsync(int count = 10);
}

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
                        CreatedById = a.UserId,
                        CreatedAt = a.Timestamp,
                        Description = a.Description ?? "",
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
                    UserContributionLevel = u.UserContributionLevel,
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
/// Client-side wrapper service for analytics - with fallback to sample data in read-only mode
/// </summary>
public class AnalyticsService(
    ApiAnalyticsService apiAnalyticsService,
    SampleDataService sampleDataService,
    ILogger<AnalyticsService> logger,
    IOptionsMonitor<MaintenanceSettings> maintenanceOptions) : IAnalyticsService
{
    private readonly ApiAnalyticsService _apiAnalyticsService = apiAnalyticsService;
    private readonly SampleDataService _sampleDataService = sampleDataService;
    private readonly ILogger<AnalyticsService> _logger = logger;
    private readonly MaintenanceSettings _maintenanceSettings = maintenanceOptions.CurrentValue;

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetDashboardStatsAsync();
        }

        return await _apiAnalyticsService.GetDashboardStatsAsync();
    }

    public async Task<List<TrendingProposalDto>> GetTrendingProposalsAsync(int hours = 24)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            // Convert sample proposals to trending format
            var sampleProposals = await _sampleDataService.GetTrendingProposalsAsync(5);
            return [.. sampleProposals.Select(p => new TrendingProposalDto
            {
                Proposal = p,
                RecentVotes = p.TotalVotes / 10,
                RecentComments = Random.Shared.Next(5, 25), // Simulate comments count
                TrendScore = p.TotalVotes + Random.Shared.Next(5, 25)
            })];
        }

        return await _apiAnalyticsService.GetTrendingProposalsAsync(hours);
    }

    public async Task<List<TopContributorDto>> GetTopContributorsAsync(int count = 10)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return
            [
                new() { UserId = "1", UserDisplayName = "Pierre Écolo", UserContributionLevel = ContributionLevel.NicolasSupreme, ProposalsCount = 15, VotesCount = 342, CommentsCount = 89, TotalScore = 2547 },
                new() { UserId = "2", UserDisplayName = "Marie Dubois", UserContributionLevel = ContributionLevel.GrosNicolas, ProposalsCount = 12, VotesCount = 298, CommentsCount = 156, TotalScore = 2134 },
                new() { UserId = "3", UserDisplayName = "Jean Travailleur", UserContributionLevel = ContributionLevel.GrosMoyenNicolas, ProposalsCount = 8, VotesCount = 187, CommentsCount = 67, TotalScore = 1456 }
            ];
        }

        return await _apiAnalyticsService.GetTopContributorsAsync(count);
    }
}