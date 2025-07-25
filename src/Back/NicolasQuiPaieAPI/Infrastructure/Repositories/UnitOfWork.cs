namespace NicolasQuiPaieAPI.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public IProposalRepository Proposals { get; } = new ProposalRepository(context);
    public IVoteRepository Votes { get; } = new VoteRepository(context);
    public ICommentRepository Comments { get; } = new CommentRepository(context);
    public ICategoryRepository Categories { get; } = new CategoryRepository(context);
    public IUserRepository Users { get; } = new UserRepository(context);
    public IApiLogRepository ApiLogs { get; } = new ApiLogRepository(context);

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}