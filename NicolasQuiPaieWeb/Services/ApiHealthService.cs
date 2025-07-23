namespace NicolasQuiPaieWeb.Services;

/// <summary>
/// Service to check API health and connectivity
/// </summary>
public class ApiHealthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiHealthService> _logger;
    private bool? _lastHealthStatus;
    private DateTime _lastHealthCheck = DateTime.MinValue;
    private readonly TimeSpan _healthCheckInterval = TimeSpan.FromMinutes(1);

    public ApiHealthService(HttpClient httpClient, ILogger<ApiHealthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Checks if the API is available and responsive
    /// </summary>
    public async Task<bool> IsApiAvailableAsync()
    {
        // Use cached result if recent
        if (_lastHealthStatus.HasValue && 
            DateTime.UtcNow - _lastHealthCheck < _healthCheckInterval)
        {
            return _lastHealthStatus.Value;
        }

        try
        {
            _logger.LogInformation("Checking API health at {BaseAddress}", _httpClient.BaseAddress);
            
            // Try a simple health check endpoint first
            using var response = await _httpClient.GetAsync("/health", HttpCompletionOption.ResponseHeadersRead);
            
            _lastHealthStatus = response.IsSuccessStatusCode;
            _lastHealthCheck = DateTime.UtcNow;
            
            if (_lastHealthStatus.Value)
            {
                _logger.LogInformation("API health check successful");
            }
            else
            {
                _logger.LogWarning("API health check failed with status: {StatusCode}", response.StatusCode);
            }
            
            return _lastHealthStatus.Value;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "API health check failed - HttpRequestException: {Message}", ex.Message);
            _lastHealthStatus = false;
            _lastHealthCheck = DateTime.UtcNow;
            return false;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning("API health check timed out");
            _lastHealthStatus = false;
            _lastHealthCheck = DateTime.UtcNow;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during API health check");
            _lastHealthStatus = false;
            _lastHealthCheck = DateTime.UtcNow;
            return false;
        }
    }

    /// <summary>
    /// Gets a user-friendly status message
    /// </summary>
    public async Task<string> GetApiStatusMessageAsync()
    {
        var isAvailable = await IsApiAvailableAsync();
        
        return isAvailable switch
        {
            true => "? API connectée et fonctionnelle",
            false => $"? API non disponible ({_httpClient.BaseAddress})"
        };
    }

    /// <summary>
    /// Forces a fresh health check
    /// </summary>
    public async Task<bool> RefreshHealthStatusAsync()
    {
        _lastHealthStatus = null;
        _lastHealthCheck = DateTime.MinValue;
        return await IsApiAvailableAsync();
    }
}