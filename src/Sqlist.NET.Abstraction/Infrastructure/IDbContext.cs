using System.Data.Common;

namespace Sqlist.NET.Infrastructure;
public interface IDbContext : IQueryStore, ICommandProvider, IDisposable, IAsyncDisposable
{
    DbConnection? Connection { get; }

    /// <summary>
    ///     Gets or sets the pending transaction, if any.
    /// </summary>
    DbTransaction? Transaction { get; }

    /// <summary>
    ///     Gets the default database of the DB provider.
    /// </summary>
    string? DefaultDatabase { get; }

    /// <summary>
    ///     Gets the <see cref="TypeMapper"/> implementation of the DB provider.
    /// </summary>
    ITypeMapper TypeMapper { get; }

    /// <summary>
    ///     Invokes the shared connection, if not doesn't already exit.
    /// </summary>
    /// <param name="revoke">The flag indicating whether to inforce connection revoke.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    Task InvokeConnectionAsync(bool revoke = false, CancellationToken cancellationToken = default);

    /// <summary>Invokes a new <see cref="DbConnection"/>.</summary>
    /// <param name="connectionString">A connection string of another data source to connection to.</param>
    /// <returns>
    ///     The <see cref="Task"/> that represents the asynchronous operation, containing the invoked <see cref="DbConnection"/>.
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// </returns>
    /// <exception cref="DbConnectionException"></exception>
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);

    DbConnection CreateConnection();

    /// <summary>
    ///     Builds and returns <see cref="DbDataSource"/> of the related DB provider.
    /// </summary>
    /// <param name="connectionString">The connection string of the DB provider to be used.</param>
    /// <returns>Returns the <see cref="DbDataSource"/> of the related DB provider.</returns>
    DbDataSource BuildDataSource(string connectionString);

    /// <summary>
    ///     Returns a new version of the internal connection string with the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The database to modify the internal connection string to.</param>
    /// <returns>A new version of the internal connection string with the specified <paramref name="database"/></returns>
    string ChangeDatabase(string database);

    /// <summary>
    ///     Starts a database transaction.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    ///     Starts a database transaction.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits the database transaction.
    /// </summary>
    void CommitTransaction();

    /// <summary>
    ///     Commits the database transaction.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rolls back a transaction from a pending state.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    ///     Rolls back a transaction from a pending state.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns a new instance of the <see cref="IQueryStore"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="IQueryStore"/>.</returns>
    IQueryStore Query();

    /// <summary>
    ///     Terminates remote connections and clears the connection pool of all related connection instances.
    /// </summary>
    /// <remarks>
    ///     If the currently connected database is the same as the given <paramref name="database"/>, the connection is switched to the provider-default.
    /// </remarks>
    /// <param name="database">The database whose connections are to be terminated.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    Task TerminateDatabaseConnectionsAsync(string database, CancellationToken cancellationToken = default);
}
