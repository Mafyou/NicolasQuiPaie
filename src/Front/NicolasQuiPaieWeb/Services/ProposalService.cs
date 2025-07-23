using NicolasQuiPaieData.DTOs;

namespace NicolasQuiPaieWeb.Services
{
    /// <summary>
    /// Client-side wrapper service for API calls - replaces the old database-dependent ProposalService
    /// </summary>
    public class ProposalService
    {
        private readonly ApiProposalService _apiProposalService;
        private readonly ILogger<ProposalService> _logger;

        public ProposalService(ApiProposalService apiProposalService, ILogger<ProposalService> logger)
        {
            _apiProposalService = apiProposalService;
            _logger = logger;
        }

        public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
        {
            return await _apiProposalService.GetActiveProposalsAsync(skip, take, category, search);
        }

        public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
        {
            return await _apiProposalService.GetTrendingProposalsAsync(take);
        }

        public async Task<ProposalDto?> GetProposalDtoByIdAsync(int id)
        {
            return await _apiProposalService.GetProposalDtoByIdAsync(id);
        }

        public async Task<ProposalDto?> CreateProposalAsync(CreateProposalDto proposal)
        {
            return await _apiProposalService.CreateProposalAsync(proposal);
        }

        public async Task IncrementViewsAsync(int proposalId)
        {
            await _apiProposalService.IncrementViewsAsync(proposalId);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            return await _apiProposalService.GetCategoriesAsync();
        }
    }
}