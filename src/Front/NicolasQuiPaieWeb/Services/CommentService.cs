namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Client-side service for comment operations via API
/// </summary>
public class ApiCommentService(HttpClient httpClient, ILogger<ApiCommentService> logger)
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Récupère tous les commentaires d'une proposition
    /// </summary>
    public async Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId)
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<List<CommentDto>>($"/api/comments/proposal/{proposalId}", _jsonOptions);
            return response ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des commentaires pour la proposition {ProposalId}", proposalId);
            return [];
        }
    }

    /// <summary>
    /// Crée un nouveau commentaire
    /// </summary>
    public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/comments", createDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDto>(_jsonOptions);
            }

            logger.LogWarning("Échec de la création du commentaire. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la création du commentaire");
            return null;
        }
    }

    /// <summary>
    /// Met à jour un commentaire existant
    /// </summary>
    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateDto)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"/api/comments/{commentId}", updateDto, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CommentDto>(_jsonOptions);
            }

            logger.LogWarning("Échec de la mise à jour du commentaire {CommentId}. Status: {StatusCode}", commentId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la mise à jour du commentaire {CommentId}", commentId);
            return null;
        }
    }

    /// <summary>
    /// Supprime un commentaire
    /// </summary>
    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"/api/comments/{commentId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la suppression du commentaire {CommentId}", commentId);
            return false;
        }
    }
}

/// <summary>
/// Client-side wrapper service for comment operations with read-only mode support
/// </summary>
public class CommentService(
    ApiCommentService apiCommentService,
    SampleDataService sampleDataService,
    ILogger<CommentService> logger,
    IOptionsMonitor<MaintenanceSettings> maintenanceOptions)
{
    private readonly ApiCommentService _apiCommentService = apiCommentService;
    private readonly SampleDataService _sampleDataService = sampleDataService;
    private readonly ILogger<CommentService> _logger = logger;
    private readonly MaintenanceSettings _maintenanceSettings = maintenanceOptions.CurrentValue;

    public async Task<IEnumerable<CommentDto>> GetCommentsForProposalAsync(int proposalId)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogInformation("Using sample data for comments (read-only mode)");
            return await _sampleDataService.GetCommentsForProposalAsync(proposalId);
        }

        return await _apiCommentService.GetCommentsForProposalAsync(proposalId);
    }

    public async Task<CommentDto?> CreateCommentAsync(CreateCommentDto createDto)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogWarning("Cannot create comment in read-only mode");
            throw new InvalidOperationException("La création de commentaires n'est pas disponible en mode démonstration.");
        }

        return await _apiCommentService.CreateCommentAsync(createDto);
    }

    public async Task<CommentDto?> UpdateCommentAsync(int commentId, UpdateCommentDto updateDto)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogWarning("Cannot update comment in read-only mode");
            throw new InvalidOperationException("La modification de commentaires n'est pas disponible en mode démonstration.");
        }

        return await _apiCommentService.UpdateCommentAsync(commentId, updateDto);
    }

    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        if (_maintenanceSettings.IsReadOnlyMode)
        {
            _logger.LogWarning("Cannot delete comment in read-only mode");
            throw new InvalidOperationException("La suppression de commentaires n'est pas disponible en mode démonstration.");
        }

        return await _apiCommentService.DeleteCommentAsync(commentId);
    }
}