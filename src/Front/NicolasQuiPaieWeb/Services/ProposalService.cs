namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Client-side wrapper service for API calls - with fallback to sample data in read-only mode
/// </summary>
public class ProposalService(
    ApiProposalService apiProposalService,
    SampleDataService sampleDataService,
    ILogger<ProposalService> logger,
    IOptionsMonitor<MaintenanceSettings> maintenanceOptions)
{
    private readonly ApiProposalService _apiProposalService = apiProposalService;
    private readonly SampleDataService _sampleDataService = sampleDataService;
    private readonly ILogger<ProposalService> _logger = logger;
    private readonly MaintenanceSettings _maintenanceSettings = maintenanceOptions.CurrentValue;

    public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetActiveProposalsAsync(skip, take, category, search);
        }

        return await _apiProposalService.GetActiveProposalsAsync(skip, take, category, search);
    }

    public async Task<IEnumerable<ProposalDto>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetRecentProposalsAsync(skip, take, category, search);
        }

        return await _apiProposalService.GetRecentProposalsAsync(skip, take, category, search);
    }

    public async Task<IEnumerable<ProposalDto>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetPopularProposalsAsync(skip, take, category, search);
        }

        return await _apiProposalService.GetPopularProposalsAsync(skip, take, category, search);
    }

    public async Task<IEnumerable<ProposalDto>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetControversialProposalsAsync(skip, take, category, search);
        }

        return await _apiProposalService.GetControversialProposalsAsync(skip, take, category, search);
    }

    public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetTrendingProposalsAsync(take);
        }

        return await _apiProposalService.GetTrendingProposalsAsync(take);
    }

    public async Task<ProposalDto?> GetProposalDtoByIdAsync(int id)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetProposalDtoByIdAsync(id);
        }

        return await _apiProposalService.GetProposalDtoByIdAsync(id);
    }

    public async Task<ProposalDto?> CreateProposalAsync(CreateProposalDto proposal)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogWarning("Cannot create proposal in read-only mode");
            throw new InvalidOperationException("La création de propositions n'est pas disponible en mode démonstration.");
        }

        return await _apiProposalService.CreateProposalAsync(proposal);
    }

    public async Task IncrementViewsAsync(int proposalId)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogDebug("Skipping view increment in read-only mode for proposal {Id}", proposalId);
            return;
        }

        await _apiProposalService.IncrementViewsAsync(proposalId);
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            return await _sampleDataService.GetCategoriesAsync();
        }

        return await _apiProposalService.GetCategoriesAsync();
    }

    public bool IsReadOnlyMode => _maintenanceSettings.IsReadOnlyMode;
}