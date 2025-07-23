using NicolasQuiPaieData.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace NicolasQuiPaieWeb.Services
{
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
        /// R�cup�re les propositions actives depuis l'API
        /// </summary>
        public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
        {
            try
            {
                var queryParams = new List<string>();
                queryParams.Add($"skip={skip}");
                queryParams.Add($"take={take}");

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
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des propositions actives");
                return new List<ProposalDto>();
            }
        }

        /// <summary>
        /// R�cup�re les propositions tendances depuis l'API
        /// </summary>
        public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
        {
            try
            {
                var url = $"/api/proposals/trending?take={take}";
                var response = await _httpClient.GetFromJsonAsync<List<ProposalDto>>(url, _jsonOptions);
                return response ?? new List<ProposalDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des propositions tendances");
                return new List<ProposalDto>();
            }
        }

        /// <summary>
        /// R�cup�re une proposition sp�cifique par ID depuis l'API
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
                _logger.LogError(ex, "Erreur lors de la r�cup�ration de la proposition {ProposalId}", id);
                return null;
            }
        }

        /// <summary>
        /// Cr�e une nouvelle proposition via l'API
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
                    _logger.LogWarning("�chec de la cr�ation de proposition. Status: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la cr�ation de la proposition");
                return null;
            }
        }

        /// <summary>
        /// Incr�mente le nombre de vues d'une proposition via l'API
        /// </summary>
        public async Task IncrementViewsAsync(int proposalId)
        {
            try
            {
                var url = $"/api/proposals/{proposalId}/views";
                var response = await _httpClient.PostAsync(url, null);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("�chec de l'incr�mentation des vues pour la proposition {ProposalId}. Status: {StatusCode}",
                                     proposalId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'incr�mentation des vues pour la proposition {ProposalId}", proposalId);
            }
        }

        /// <summary>
        /// R�cup�re toutes les cat�gories actives depuis l'API
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("/api/categories", _jsonOptions);
                return response ?? new List<CategoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la r�cup�ration des cat�gories");
                return new List<CategoryDto>();
            }
        }
    }
}