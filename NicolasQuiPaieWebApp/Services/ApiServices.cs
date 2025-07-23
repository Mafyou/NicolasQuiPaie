using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using NicolasQuiPaieData.DTOs;
using System.Net.Http.Json;

namespace NicolasQuiPaieWebApp.Services
{
    public class ProposalApiService : IProposalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAccessTokenProvider _tokenProvider;

        public ProposalApiService(IHttpClientFactory httpClientFactory, IAccessTokenProvider tokenProvider)
        {
            _httpClient = httpClientFactory.CreateClient("NicolasQuiPaieAPI");
            _tokenProvider = tokenProvider;
        }

        public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null)
        {
            var query = $"?skip={skip}&take={take}";
            if (categoryId.HasValue) query += $"&categoryId={categoryId}";
            if (!string.IsNullOrEmpty(search)) query += $"&search={Uri.EscapeDataString(search)}";

            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ProposalDto>>($"api/proposals{query}");
            return response ?? new List<ProposalDto>();
        }

        public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<ProposalDto>>($"api/proposals/trending?take={take}");
            return response ?? new List<ProposalDto>();
        }

        public async Task<ProposalDto?> GetProposalByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ProposalDto>($"api/proposals/{id}");
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return null;
            }
        }

        public async Task<ProposalDto> CreateProposalAsync(CreateProposalDto createDto)
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.PostAsJsonAsync("api/proposals", createDto);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<ProposalDto>() 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<ProposalDto> UpdateProposalAsync(int id, UpdateProposalDto updateDto)
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.PutAsJsonAsync($"api/proposals/{id}", updateDto);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<ProposalDto>() 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task DeleteProposalAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.DeleteAsync($"api/proposals/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> CanUserEditProposalAsync(int proposalId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                
                var response = await _httpClient.GetAsync($"api/proposals/{proposalId}/can-edit");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<bool>();
                    return result;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var tokenResult = await _tokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);
            }
        }
    }

    public class VotingApiService : IVotingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAccessTokenProvider _tokenProvider;

        public VotingApiService(IHttpClientFactory httpClientFactory, IAccessTokenProvider tokenProvider)
        {
            _httpClient = httpClientFactory.CreateClient("NicolasQuiPaieAPI");
            _tokenProvider = tokenProvider;
        }

        public async Task<VoteDto> CastVoteAsync(CreateVoteDto voteDto)
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.PostAsJsonAsync("api/votes", voteDto);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<VoteDto>() 
                ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        public async Task<VoteDto?> GetUserVoteForProposalAsync(int proposalId)
        {
            try
            {
                await SetAuthorizationHeaderAsync();
                
                var response = await _httpClient.GetFromJsonAsync<VoteDto>($"api/votes/proposal/{proposalId}/user");
                return response;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                return null;
            }
        }

        public async Task RemoveVoteAsync(int proposalId)
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.DeleteAsync($"api/votes/proposal/{proposalId}/user");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<VoteDto>> GetVotesForProposalAsync(int proposalId)
        {
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<VoteDto>>($"api/votes/proposal/{proposalId}");
            return response ?? new List<VoteDto>();
        }

        public async Task<IEnumerable<VoteDto>> GetUserVotesAsync()
        {
            await SetAuthorizationHeaderAsync();
            
            var response = await _httpClient.GetFromJsonAsync<IEnumerable<VoteDto>>("api/votes/user");
            return response ?? new List<VoteDto>();
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var tokenResult = await _tokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Value);
            }
        }
    }
}