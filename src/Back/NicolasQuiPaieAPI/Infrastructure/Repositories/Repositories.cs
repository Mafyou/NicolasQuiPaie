namespace NicolasQuiPaieAPI.Infrastructure.Repositories;

public class BaseRepository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await _dbSet.AddAsync(entity);
        return result.Entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return entity;
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
}

public class ProposalRepository(ApplicationDbContext context) : BaseRepository<Proposal>(context), IProposalRepository
{
    public async Task<IEnumerable<Proposal>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null)
    {
        var query = _context.Proposals
            .Include(p => p.CreatedBy)
            .Include(p => p.Category)
            .Where(p => p.Status == ProposalStatus.Active);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetTrendingProposalsAsync(int take = 5)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-7);

        return await _context.Proposals
            .Include(p => p.CreatedBy)
            .Include(p => p.Category)
            .Where(p => p.Status == ProposalStatus.Active && p.CreatedAt >= cutoffDate)
            .OrderByDescending(p => p.VotesFor + p.VotesAgainst)
            .ThenByDescending(p => p.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        var query = _context.Proposals
            .Include(p => p.CreatedBy)
            .Include(p => p.Category)
            .Where(p => p.Status == ProposalStatus.Active);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Name.Contains(category));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        var query = _context.Proposals
            .Include(p => p.CreatedBy)
            .Include(p => p.Category)
            .Where(p => p.Status == ProposalStatus.Active);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Name.Contains(category));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
        }

        return await query
            .OrderByDescending(p => p.VotesFor)
            .ThenByDescending(p => p.VotesFor + p.VotesAgainst)
            .ThenByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null)
    {
        var query = _context.Proposals
            .Include(p => p.CreatedBy)
            .Include(p => p.Category)
            .Where(p => p.Status == ProposalStatus.Active)
            .Where(p => p.VotesFor > 0 && p.VotesAgainst > 0); // Only proposals with both types of votes

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category.Name.Contains(category));
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search));
        }

        // Order by controversy score: closer to 50% ratio = more controversial
        // Also prioritize proposals with more total votes
        return await query
            .OrderBy(p => Math.Abs(0.5 - (double)p.VotesFor / (p.VotesFor + p.VotesAgainst)))
            .ThenByDescending(p => p.VotesFor + p.VotesAgainst)
            .ThenByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetUserProposalsAsync(string userId)
    {
        return await _context.Proposals
            .Include(p => p.Category)
            .Where(p => p.CreatedById == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateVoteCountsAsync(int proposalId)
    {
        var proposal = await GetByIdAsync(proposalId);
        if (proposal != null)
        {
            var votesFor = await _context.Votes
                .CountAsync(v => v.ProposalId == proposalId && v.VoteType == VoteType.For);

            var votesAgainst = await _context.Votes
                .CountAsync(v => v.ProposalId == proposalId && v.VoteType == VoteType.Against);

            proposal.VotesFor = votesFor;
            proposal.VotesAgainst = votesAgainst;
        }
    }
}

public class VoteRepository(ApplicationDbContext context) : BaseRepository<Vote>(context), IVoteRepository
{
    public async Task<Vote?> GetUserVoteForProposalAsync(string userId, int proposalId)
    {
        return await _context.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.ProposalId == proposalId);
    }

    public async Task<IEnumerable<Vote>> GetVotesForProposalAsync(int proposalId)
    {
        return await _context.Votes
            .Include(v => v.User)
            .Where(v => v.ProposalId == proposalId)
            .OrderByDescending(v => v.VotedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vote>> GetUserVotesAsync(string userId)
    {
        return await _context.Votes
            .Include(v => v.Proposal)
            .ThenInclude(p => p.Category)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.VotedAt)
            .ToListAsync();
    }
}

public class CommentRepository(ApplicationDbContext context) : BaseRepository<Comment>(context), ICommentRepository
{
    public async Task<IEnumerable<Comment>> GetCommentsForProposalAsync(int proposalId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Where(c => c.ProposalId == proposalId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId)
    {
        return await _context.Comments
            .Include(c => c.Proposal)
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetRepliesAsync(int parentCommentId)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}

public class CategoryRepository(ApplicationDbContext context) : BaseRepository<Category>(context), ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await context.Users.FindAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<ApplicationUser> UpdateAsync(ApplicationUser user)
    {
        context.Users.Update(user);
        return user;
    }

    public async Task<IEnumerable<ApplicationUser>> GetTopContributorsAsync(int take = 10)
    {
        return await context.Users
            .OrderByDescending(u => u.ReputationScore)
            .Take(take)
            .ToListAsync();
    }
}

public class ApiLogRepository(ApplicationDbContext context) : BaseRepository<ApiLog>(context), IApiLogRepository
{
    public async Task<IEnumerable<ApiLog>> GetLatestLogsAsync(int take = 100, NicolasQuiPaieAPI.Infrastructure.Models.LogLevel? level = null)
    {
        var query = _context.ApiLogs.AsQueryable();
        
        if (level.HasValue)
        {
            // Filter for logs that are at least as critical as the specified level
            // (level value or higher in the enum hierarchy)
            query = query.Where(l => l.Level >= level.Value);
        }

        return await query
            .OrderByDescending(l => l.TimeStamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApiLog>> GetLogsByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level, int take = 100)
    {
        return await _context.ApiLogs
            .Where(l => l.Level == level)
            .OrderByDescending(l => l.TimeStamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApiLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int take = 100)
    {
        return await _context.ApiLogs
            .Where(l => l.TimeStamp >= startDate && l.TimeStamp <= endDate)
            .OrderByDescending(l => l.TimeStamp)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetLogCountByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level)
    {
        return await _context.ApiLogs
            .CountAsync(l => l.Level == level);
    }

    public async Task<DateTime?> GetOldestLogDateAsync()
    {
        return await _context.ApiLogs
            .OrderBy(l => l.TimeStamp)
            .Select(l => l.TimeStamp)
            .FirstOrDefaultAsync();
    }

    public async Task<DateTime?> GetNewestLogDateAsync()
    {
        return await _context.ApiLogs
            .OrderByDescending(l => l.TimeStamp)
            .Select(l => l.TimeStamp)
            .FirstOrDefaultAsync();
    }
}