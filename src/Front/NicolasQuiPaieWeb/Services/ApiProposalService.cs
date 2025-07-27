namespace NicolasQuiPaieWeb.Services;

public class ApiProposalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiProposalService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiProposalService(HttpClient httpClient, ILogger<ApiProposalService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Récupère les propositions actives depuis l'API
    /// </summary>
    public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"skip={skip}",
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var queryString = string.Join("&", queryParams);
            var url = $"/api/proposals?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
            return response ?? new List<ProposalDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions actives");
            return [];
        }
    }

    /// <summary>
    /// Récupère les propositions récentes depuis l'API
    /// </summary>
    public async Task<IEnumerable<ProposalDto>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"skip={skip}",
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var queryString = string.Join("&", queryParams);
            var url = $"/api/proposals/recent?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions récentes");
            return [];
        }
    }

    /// <summary>
    /// Récupère les propositions populaires depuis l'API
    /// </summary>
    public async Task<IEnumerable<ProposalDto>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"skip={skip}",
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var queryString = string.Join("&", queryParams);
            var url = $"/api/proposals/popular?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions populaires");
            return [];
        }
    }

    /// <summary>
    /// Récupère les propositions controversées depuis l'API
    /// </summary>
    public async Task<IEnumerable<ProposalDto>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"skip={skip}",
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var queryString = string.Join("&", queryParams);
            var url = $"/api/proposals/controversial?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions controversées");
            return [];
        }
    }

    /// <summary>
    /// Récupère les propositions tendances depuis l'API
    /// </summary>
    public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
    {
        try
        {
            var url = $"/api/proposals/trending?take={take}";
            var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des propositions tendances");
            return [];
        }
    }

    /// <summary>
    /// Récupère une proposition spécifique par ID depuis l'API
    /// </summary>
    public async Task<ProposalDto?> GetProposalDtoByIdAsync(int id)
    {
        try
        {
            var url = $"/api/proposals/{id}";
            return await _httpClient.GetFromJsonAsync<ProposalDto>(url, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la proposition {ProposalId}", id);
            return null;
        }
    }

    /// <summary>
    /// Crée une nouvelle proposition via l'API
    /// </summary>
    public async Task<ProposalDto?> CreateProposalAsync(CreateProposalDto createProposalDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/proposals", createProposalDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProposalDto>(_jsonOptions);
            }
            else
            {
                _logger.LogWarning("Échec de la création de proposition. Status: {StatusCode}", response.StatusCode);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la proposition");
            return null;
        }
    }

    /// <summary>
    /// C# 13.0 - Toggle proposal status (SuperUser/Admin only)
    /// </summary>
    public async Task<ProposalDto?> ToggleProposalStatusAsync(int proposalId, ProposalStatus newStatus)
    {
        try
        {
            var statusDto = new ToggleProposalStatusDto { NewStatus = newStatus };
            var url = $"/api/proposals/{proposalId}/status";

            var response = await _httpClient.PatchAsJsonAsync(url, statusDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var updatedProposal = await response.Content.ReadFromJsonAsync<ProposalDto>(_jsonOptions);
                _logger.LogInformation("Proposal status toggled successfully: {ProposalId} -> {NewStatus}", proposalId, newStatus);
                return updatedProposal;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("Access denied for proposal status toggle: {ProposalId}. User lacks SuperUser/Admin role", proposalId);
                return null;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Proposal not found for status toggle: {ProposalId}", proposalId);
                return null;
            }
            else
            {
                _logger.LogWarning("Failed to toggle proposal status: {ProposalId}. Status: {StatusCode}", proposalId, response.StatusCode);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while toggling proposal status: {ProposalId}", proposalId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while toggling proposal status: {ProposalId}", proposalId);
            return null;
        }
    }

    /// <summary>
    /// Incrémente le nombre de vues d'une proposition via l'API
    /// </summary>
    public async Task IncrementViewsAsync(int proposalId)
    {
        try
        {
            var url = $"/api/proposals/{proposalId}/views";
            var response = await _httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Views count incremented successfully for proposal {ProposalId}", proposalId);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Proposal {ProposalId} not found for view increment", proposalId);
            }
            else
            {
                _logger.LogWarning("Failed to increment views for proposal {ProposalId}. Status: {StatusCode}",
                                 proposalId, response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Network error while incrementing views for proposal {ProposalId}", proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while incrementing views for proposal {ProposalId}", proposalId);
        }
    }

    /// <summary>
    /// Récupère toutes les catégories actives depuis l'API
    /// </summary>
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("/api/categories", _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des catégories");
            return [];
        }
    }
}