namespace NicolasQuiPaieWeb.Services
{
    public class ApiVotingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiVotingService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiVotingService(HttpClient httpClient, ILogger<ApiVotingService> logger)
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
        /// Soumet un vote via l'API
        /// </summary>
        public async Task<bool> CastVoteAsync(CreateVoteDto voteDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/votes", voteDto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Vote soumis avec succès pour la proposition {ProposalId}", voteDto.ProposalId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Échec de soumission du vote. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la soumission du vote pour la proposition {ProposalId}", voteDto.ProposalId);
                return false;
            }
        }

        /// <summary>
        /// Récupère les votes d'une proposition via l'API
        /// </summary>
        public async Task<IEnumerable<VoteDto>> GetProposalVotesAsync(int proposalId)
        {
            try
            {
                var url = $"/api/votes/proposal/{proposalId}";
                var response = await _httpClient.GetFromJsonAsync<List<VoteDto>>(url, _jsonOptions);
                return response ?? new List<VoteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des votes pour la proposition {ProposalId}", proposalId);
                return new List<VoteDto>();
            }
        }

        /// <summary>
        /// Récupère les votes de l'utilisateur actuel via l'API
        /// </summary>
        public async Task<IEnumerable<VoteDto>> GetUserVotesAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<VoteDto>>($"/api/votes/user/{userId}", _jsonOptions);
                return response ?? new List<VoteDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des votes de l'utilisateur");
                return new List<VoteDto>();
            }
        }

        /// <summary>
        /// Supprime le vote de l'utilisateur pour une proposition via l'API
        /// </summary>
        public async Task<bool> DeleteUserVoteAsync(int proposalId)
        {
            try
            {
                var url = $"/api/votes/proposal/{proposalId}/user";
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Vote supprimé avec succès pour la proposition {ProposalId}", proposalId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Échec de suppression du vote. Status: {StatusCode}", response.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du vote pour la proposition {ProposalId}", proposalId);
                return false;
            }
        }
    }
}