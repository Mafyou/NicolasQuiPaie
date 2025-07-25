namespace NicolasQuiPaieAPI.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IUnitOfWork : IDisposable
{
    IProposalRepository Proposals { get; }
    IVoteRepository Votes { get; }
    ICommentRepository Comments { get; }
    ICategoryRepository Categories { get; }
    IUserRepository Users { get; }
    IApiLogRepository ApiLogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public interface IProposalRepository : IRepository<Proposal>
{
    Task<IEnumerable<Proposal>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null);
    Task<IEnumerable<Proposal>> GetTrendingProposalsAsync(int take = 5);
    Task<IEnumerable<Proposal>> GetUserProposalsAsync(string userId);
    Task UpdateVoteCountsAsync(int proposalId);
}

public interface IVoteRepository : IRepository<Vote>
{
    Task<Vote?> GetUserVoteForProposalAsync(string userId, int proposalId);
    Task<IEnumerable<Vote>> GetVotesForProposalAsync(int proposalId);
    Task<IEnumerable<Vote>> GetUserVotesAsync(string userId);
}

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetCommentsForProposalAsync(int proposalId);
    Task<IEnumerable<Comment>> GetUserCommentsAsync(string userId);
    Task<IEnumerable<Comment>> GetRepliesAsync(int parentCommentId);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    Task<IEnumerable<ApplicationUser>> GetTopContributorsAsync(int take = 10);
}

public interface IApiLogRepository : IRepository<ApiLog>
{
    /// <summary>
    /// Gets the latest logs, optionally filtering for logs at least as critical as the specified level
    /// </summary>
    /// <param name="take">Maximum number of logs to retrieve</param>
    /// <param name="level">Minimum log level (inclusive) - returns logs at this level and above</param>
    /// <returns>Collection of logs ordered by timestamp (newest first)</returns>
    Task<IEnumerable<ApiLog>> GetLatestLogsAsync(int take = 100, NicolasQuiPaieAPI.Infrastructure.Models.LogLevel? level = null);
    Task<IEnumerable<ApiLog>> GetLogsByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level, int take = 100);
    Task<IEnumerable<ApiLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int take = 100);
    Task<int> GetLogCountByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level);
    Task<DateTime?> GetOldestLogDateAsync();
    Task<DateTime?> GetNewestLogDateAsync();
}