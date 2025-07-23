using Microsoft.EntityFrameworkCore;
using NicolasQuiPaieWeb.Data;
using NicolasQuiPaieWeb.Data.Models;
using NicolasQuiPaieWeb.Data.DTOs;

namespace NicolasQuiPaieWeb.Services
{
    public class ProposalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProposalService> _logger;

        public ProposalService(ApplicationDbContext context, ILogger<ProposalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Récupère les propositions actives sous forme de DTOs pour éviter les problèmes de lazy loading
        /// </summary>
        public async Task<IEnumerable<ProposalDto>> GetActiveProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
        {
            var query = _context.Proposals
                .Include(p => p.CreatedBy)
                .Include(p => p.Category)
                .Where(p => p.Status == ProposalStatus.Active);

            if (!string.IsNullOrEmpty(category) && int.TryParse(category, out int categoryId))
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(p => new ProposalDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    VotesFor = p.VotesFor,
                    VotesAgainst = p.VotesAgainst,
                    ViewsCount = p.ViewsCount,
                    IsFeatured = p.IsFeatured,
                    ClosedAt = p.ClosedAt,
                    CreatedById = p.CreatedById,
                    CreatedByDisplayName = p.CreatedBy.DisplayName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CategoryColor = p.Category.Color,
                    CategoryIcon = p.Category.IconClass
                })
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les propositions tendances sous forme de DTOs
        /// </summary>
        public async Task<IEnumerable<ProposalDto>> GetTrendingProposalsAsync(int take = 5)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            
            return await _context.Proposals
                .Include(p => p.CreatedBy)
                .Include(p => p.Category)
                .Where(p => p.Status == ProposalStatus.Active && p.CreatedAt >= cutoffDate)
                .OrderByDescending(p => p.VotesFor + p.VotesAgainst)
                .ThenByDescending(p => p.CreatedAt)
                .Take(take)
                .Select(p => new ProposalDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    VotesFor = p.VotesFor,
                    VotesAgainst = p.VotesAgainst,
                    ViewsCount = p.ViewsCount,
                    IsFeatured = p.IsFeatured,
                    ClosedAt = p.ClosedAt,
                    CreatedById = p.CreatedById,
                    CreatedByDisplayName = p.CreatedBy.DisplayName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CategoryColor = p.Category.Color,
                    CategoryIcon = p.Category.IconClass
                })
                .ToListAsync();
        }

        /// <summary>
        /// Récupère une proposition spécifique - retourne l'entité pour les opérations de modification
        /// </summary>
        public async Task<Proposal?> GetProposalByIdAsync(int id)
        {
            return await _context.Proposals
                .Include(p => p.CreatedBy)
                .Include(p => p.Category)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                .Include(p => p.Votes)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Récupère une proposition sous forme de DTO pour l'affichage
        /// </summary>
        public async Task<ProposalDto?> GetProposalDtoByIdAsync(int id)
        {
            return await _context.Proposals
                .Include(p => p.CreatedBy)
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProposalDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    VotesFor = p.VotesFor,
                    VotesAgainst = p.VotesAgainst,
                    ViewsCount = p.ViewsCount,
                    IsFeatured = p.IsFeatured,
                    ClosedAt = p.ClosedAt,
                    CreatedById = p.CreatedById,
                    CreatedByDisplayName = p.CreatedBy.DisplayName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    CategoryColor = p.Category.Color,
                    CategoryIcon = p.Category.IconClass
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crée une nouvelle proposition
        /// </summary>
        public async Task<Proposal> CreateProposalAsync(Proposal proposal)
        {
            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();
            return proposal;
        }

        /// <summary>
        /// Incrémente le nombre de vues d'une proposition
        /// </summary>
        public async Task IncrementViewsAsync(int proposalId)
        {
            var proposal = await _context.Proposals.FindAsync(proposalId);
            if (proposal != null)
            {
                proposal.ViewsCount++;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Récupère toutes les catégories actives
        /// </summary>
        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
        }
    }
}