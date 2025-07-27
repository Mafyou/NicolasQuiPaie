using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace NicolasQuiPaieWeb.Authentication
{
    /// <summary>
    /// C# 13.0 - Enhanced JWT Authentication State Provider with proper role support for Blazor AuthorizeView
    /// </summary>
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<JwtAuthenticationStateProvider> _logger;

        public JwtAuthenticationStateProvider(
            ILocalStorageService localStorage,
            HttpClient httpClient,
            ILogger<JwtAuthenticationStateProvider> logger)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("No auth token found in local storage");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // ?? Ensure JWT role claims are not mapped incorrectly
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                // Validate token is not expired
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    _logger.LogWarning("JWT token has expired, removing from storage");
                    await _localStorage.RemoveItemAsync("authToken");
                    await _localStorage.RemoveItemAsync("refreshToken");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Set Authorization header for API calls
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ?? Create claims with proper role handling for AuthorizeView
                var claims = new List<Claim>();
                
                foreach (var claim in jwtToken.Claims)
                {
                    // ?? CRITICAL: Map JWT "role" claims to ClaimTypes.Role for AuthorizeView compatibility
                    if (claim.Type == "role")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                        _logger.LogDebug("Mapped role claim: {Role}", claim.Value);
                    }
                    else
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                    }
                }

                // ?? Create ClaimsIdentity with RoleClaimType specified for proper IsInRole() behavior
                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);

                // ?? Log user roles for debugging
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
                
                _logger.LogInformation("User authenticated: {Email} with roles: {Roles}", 
                    userEmail, string.Join(", ", userRoles));

                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authentication state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task MarkUserAsAuthenticated(string token, string refreshToken)
        {
            try
            {
                await _localStorage.SetItemAsync("authToken", token);
                await _localStorage.SetItemAsync("refreshToken", refreshToken);

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ?? Clear JWT claim mappings to ensure proper role handling
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                // ?? Create claims with proper role handling
                var claims = new List<Claim>();
                
                foreach (var claim in jwtToken.Claims)
                {
                    // ?? CRITICAL: Map JWT "role" claims to ClaimTypes.Role for AuthorizeView compatibility
                    if (claim.Type == "role")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
                        _logger.LogDebug("Added role claim during login: {Role}", claim.Value);
                    }
                    else
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                    }
                }

                // ?? Create ClaimsIdentity with RoleClaimType specified for proper IsInRole() behavior
                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);
                var user = new ClaimsPrincipal(identity);

                // ?? Log successful authentication with roles
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
                
                _logger.LogInformation("User marked as authenticated: {Email} with roles: {Roles}", 
                    userEmail, string.Join(", ", userRoles));

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking user as authenticated");
            }
        }

        public async Task MarkUserAsLoggedOut()
        {
            try
            {
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");

                _httpClient.DefaultRequestHeaders.Authorization = null;

                var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
                
                _logger.LogInformation("User logged out successfully");
                
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking user as logged out");
            }
        }

        /// <summary>
        /// C# 13.0 - Force refresh authentication state
        /// </summary>
        public async Task RefreshAuthenticationStateAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        /// <summary>
        /// C# 13.0 - Get current user roles
        /// </summary>
        public async Task<List<string>> GetCurrentUserRolesAsync()
        {
            var authState = await GetAuthenticationStateAsync();
            return authState.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }

        /// <summary>
        /// C# 13.0 - Check if current user has specific role
        /// </summary>
        public async Task<bool> HasRoleAsync(string role)
        {
            var authState = await GetAuthenticationStateAsync();
            return authState.User.IsInRole(role);
        }
    }
}