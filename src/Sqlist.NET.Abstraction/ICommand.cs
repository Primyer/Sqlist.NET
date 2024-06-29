using System.Data.Common;
using System.Data;
using Sqlist.NET.Utilities;

namespace Sqlist.NET;
public interface ICommand : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     Gets the underlying <see cref="DbCommand"/>.
    /// </summary>
    DbCommand Underlying { get; }

    /// <summary>
    ///     Gets or sets the SQL statement to run against the data source.
    /// </summary>
    string Statement { get; set; }

    /// <summary>
    ///     Gets or sets the parameters associated with the given configured statement.
    /// </summary>
    object? Parameters { get; set; }

    /// <summary>
    ///     Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
    /// </summary>
    int? Timeout { get; set; }

    /// <summary>
    ///     Gets or sets the type that indicates how SQL statement is interpreted.
    /// </summary>
    CommandType? Type { get; set; }


    Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default);

    Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Prepares <see cref="DbDataReader"/> by returning a <see cref="LazyDbDataReader"/> that delays enumeration.
    /// </summary>
    /// <param name="commandBehavior">One of the <see cref="CommandBehavior"/> values.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The <see cref="ILazyDataReader"/> object tahat delays enumertions.</returns>
    ILazyDataReader PrepareReader(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default);

    /// <summary>Invokes <see cref="DbDataReader"/>.</summary>
    /// <param name="commandBehavior">One of the <see cref="CommandBehavior"/> values.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> that represent the asynchronous operation, containing the invoked <see cref="DbDataReader"/>.
    /// </returns>
    Task<DbDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default);
}
