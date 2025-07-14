namespace Sqlist.NET;
public interface ITransactionManager
{

    /// <summary>
    ///     Starts a database transaction.
    /// </summary>
    void Begin();

    /// <summary>
    ///     Starts a database transaction.
    /// </summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits a database transaction.
    /// </summary>
    void Commit();

    /// <summary>
    ///     Commits a database transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rolls back a transaction from a pending state.
    /// </summary>
    void Rollback();

    /// <summary>
    ///     Rolls back a transaction from a pending state.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
