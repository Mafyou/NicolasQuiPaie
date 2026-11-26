namespace NicolasQuiPaieWeb.Services;

public class ApiLogsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiLogsService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiLogsService(HttpClient httpClient, ILogger<ApiLogsService> logger)
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
    /// Récupère les logs depuis l'API avec filtrage optionnel par niveau
    /// </summary>
    public async Task<ApiLogsResponseDto?> GetLogsAsync(string? level = null, int take = 100)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"take={take}"
            };

            if (!string.IsNullOrEmpty(level))
                queryParams.Add($"level={Uri.EscapeDataString(level)}");

            var queryString = string.Join("&", queryParams);
            var url = $"/api/logs?{queryString}";

            var response = await _httpClient.GetFromJsonAsync<ApiLogsResponseDto>(url, _jsonOptions);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des logs avec niveau: {Level}, take: {Take}", level, take);
            return null;
        }
    }

    /// <summary>
    /// Crée un log de test via l'API
    /// </summary>
    public async Task<bool> CreateTestLogAsync(string message, string level = "Warning")
    {
        try
        {
            var testLogRequest = new
            {
                Message = message,
                Level = level
            };

            var response = await _httpClient.PostAsJsonAsync("/api/test-logging", testLogRequest, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Test log created successfully with level {Level}", level);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to create test log. Status: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du log de test");
            return false;
        }
    }

    /// <summary>
    /// Récupère tous les niveaux de log disponibles
    /// </summary>
    public IReadOnlyList<string> GetAvailableLogLevels() => AvailableLogLevels;

    /// <summary>
    /// Obtient la couleur associée à un niveau de log
    /// </summary>
    public string GetLogLevelColor(string level) => StyleOf(level).Color;

    /// <summary>
    /// Obtient l'icône associée à un niveau de log
    /// </summary>
    public string GetLogLevelIcon(string level) => StyleOf(level).Icon;

    /// <summary>Bootstrap colour and Font Awesome icon of a Serilog level.</summary>
    private readonly record struct LevelStyle(string Color, string Icon);

    private static readonly IReadOnlyList<string> AvailableLogLevels = ["All", "Verbose", "Debug", "Information", "Warning", "Error", "Fatal"];

    private static readonly LevelStyle UnknownLevel = new("text-secondary", "fas fa-file-alt");

    // Looked up for every log row shown: built once, matched without case (ToLower() depended on the culture).
    private static readonly FrozenDictionary<string, LevelStyle> Styles = new Dictionary<string, LevelStyle>
    {
        ["Verbose"] = new("text-muted", "fas fa-comment-dots"),
        ["Debug"] = new("text-info", "fas fa-bug"),
        ["Information"] = new("text-primary", "fas fa-info-circle"),
        ["Warning"] = new("text-warning", "fas fa-exclamation-triangle"),
        ["Error"] = new("text-danger", "fas fa-times-circle"),
        ["Fatal"] = new("text-danger fw-bold", "fas fa-skull-crossbones")
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private static LevelStyle StyleOf(string? level) => level is not null && Styles.TryGetValue(level, out var style) ? style : UnknownLevel;
}