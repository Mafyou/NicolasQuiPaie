using NicolasQuiPaieData.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace NicolasQuiPaieWeb.Services
{
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
                var queryParams = new List<string>();
                queryParams.Add($"take={take}");

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
        public string[] GetAvailableLogLevels()
        {
            return ["All", "Verbose", "Debug", "Information", "Warning", "Error", "Fatal"];
        }

        /// <summary>
        /// Obtient la couleur associée à un niveau de log
        /// </summary>
        public string GetLogLevelColor(string level)
        {
            return level.ToLower() switch
            {
                "verbose" => "text-muted",
                "debug" => "text-info",
                "information" => "text-primary",
                "warning" => "text-warning",
                "error" => "text-danger",
                "fatal" => "text-danger fw-bold",
                _ => "text-secondary"
            };
        }

        /// <summary>
        /// Obtient l'icône associée à un niveau de log
        /// </summary>
        public string GetLogLevelIcon(string level)
        {
            return level.ToLower() switch
            {
                "verbose" => "fas fa-comment-dots",
                "debug" => "fas fa-bug",
                "information" => "fas fa-info-circle",
                "warning" => "fas fa-exclamation-triangle",
                "error" => "fas fa-times-circle",
                "fatal" => "fas fa-skull-crossbones",
                _ => "fas fa-file-alt"
            };
        }
    }
}