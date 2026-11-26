namespace NicolasQuiPaieAPI.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
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
    Task<IReadOnlyList<Proposal>> GetActiveProposalsAsync(int skip = 0, int take = 20, int? categoryId = null, string? search = null);
    Task<IReadOnlyList<Proposal>> GetTrendingProposalsAsync(int take = 5);
    Task<IReadOnlyList<Proposal>> GetRecentProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null);
    Task<IReadOnlyList<Proposal>> GetPopularProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null);
    Task<IReadOnlyList<Proposal>> GetControversialProposalsAsync(int skip = 0, int take = 20, string? category = null, string? search = null);
    Task<IReadOnlyList<Proposal>> GetUserProposalsAsync(string userId);
    Task UpdateVoteCountsAsync(int proposalId);
}

public interface IVoteRepository : IRepository<Vote>
{
    Task<Vote?> GetUserVoteForProposalAsync(string userId, int proposalId);
    Task<IReadOnlyList<Vote>> GetVotesForProposalAsync(int proposalId);
    Task<IReadOnlyList<Vote>> GetUserVotesAsync(string userId);
}

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetCommentsForProposalAsync(int proposalId);
    Task<IReadOnlyList<Comment>> GetUserCommentsAsync(string userId);
    Task<IReadOnlyList<Comment>> GetRepliesAsync(int parentCommentId);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync();
}

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    Task<IReadOnlyList<ApplicationUser>> GetTopContributorsAsync(int take = 10);
}

public interface IApiLogRepository : IRepository<ApiLog>
{
    /// <summary>
    /// Gets the latest logs, optionally filtering for logs at least as critical as the specified level
    /// </summary>
    /// <param name="take">Maximum number of logs to retrieve</param>
    /// <param name="level">Minimum log level (inclusive) - returns logs at this level and above</param>
    /// <returns>Collection of logs ordered by timestamp (newest first)</returns>
    Task<IReadOnlyList<ApiLog>> GetLatestLogsAsync(int take = 100, NicolasQuiPaieAPI.Infrastructure.Models.LogLevel? level = null);
    Task<IReadOnlyList<ApiLog>> GetLogsByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level, int take = 100);
    Task<IReadOnlyList<ApiLog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate, int take = 100);
    Task<int> GetLogCountByLevelAsync(NicolasQuiPaieAPI.Infrastructure.Models.LogLevel level);
    Task<DateTime?> GetOldestLogDateAsync();
    Task<DateTime?> GetNewestLogDateAsync();
}