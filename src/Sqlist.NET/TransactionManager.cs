using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

namespace Sqlist.NET;

/// <summary>
///     Provides a high-level access to the transaction API.
/// </summary>
public sealed class TransactionManager : ITransactionManager
{
    private readonly IDbContext _db;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionManager"/> class.
    /// </summary>
    /// <param name="db">The <see cref="DbContextBase"/> for the transaction API.</param>
    public TransactionManager(IDbContext db)
    {
        Check.NotNull(db);
        _db = db;
    }

    public void Begin()
    {
        BeginAsync().Wait();
    }

    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        return _db.BeginTransactionAsync(cancellationToken);
    }

    public void Commit()
    {
        CommitAsync().Wait();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _db.CommitTransactionAsync(cancellationToken);
    }

    public void Rollback()
    {
        RollbackAsync().Wait();
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _db.RollbackTransactionAsync(cancellationToken);
    }
}