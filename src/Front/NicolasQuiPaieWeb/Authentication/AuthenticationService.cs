namespace NicolasQuiPaieWeb.Authentication;

public interface IAuthenticationService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest);
    Task LogoutAsync();
    Task<bool> RefreshTokenAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly JwtAuthenticationStateProvider _authStateProvider;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthenticationService(
        HttpClient httpClient,
        AuthenticationStateProvider authStateProvider,
        ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _authStateProvider = (JwtAuthenticationStateProvider)authStateProvider;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);

                if (authResponse != null && authResponse.Success)
                {
                    await _authStateProvider.MarkUserAsAuthenticated(authResponse.Token!, authResponse.RefreshToken!);
                    return authResponse;
                }
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
            return errorResponse ?? new AuthResponseDto
            {
                Success = false,
                Errors = ["Login failed"]
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return new AuthResponseDto
            {
                Success = false,
                Errors = [$"Login error: {ex.Message}"]
            };
        }
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerRequest, _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);

                if (authResponse != null && authResponse.Success)
                {
                    await _authStateProvider.MarkUserAsAuthenticated(authResponse.Token!, authResponse.RefreshToken!);
                    return authResponse;
                }
            }

            var errorResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
            return errorResponse ?? new AuthResponseDto
            {
                Success = false,
                Errors = ["Registration failed"]
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return new AuthResponseDto
            {
                Success = false,
                Errors = [$"Registration error: {ex.Message}"]
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Call API logout endpoint
            await _httpClient.PostAsync("/api/auth/logout", null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calling logout API endpoint");
        }
        finally
        {
            // Always clear local authentication state
            await _authStateProvider.MarkUserAsLoggedOut();
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            // TODO: Implement refresh token logic when API supports it
            _logger.LogWarning("Refresh token not yet implemented");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return false;
        }
    }
}