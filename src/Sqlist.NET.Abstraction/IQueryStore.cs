using System.Data;
using System.Data.Common;

namespace Sqlist.NET;

/// <summary>
///     Provides the API to query against a data source.
/// </summary>
public interface IQueryStore
{
    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, as an asynchronous operation.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the number of rows affected.
    /// </returns>
    Task<int> ExecuteAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);
    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The number of rows affected.</returns>
    int Execute(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source,
    ///     and returns an <see cref="IEnumerable{T}"/> as the result, as an asynchronous operation.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="altr">The action to modify the generated models within the mapping process.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> as result.
    /// </returns>
    Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source,
    ///     and returns an <see cref="IEnumerable{T}"/> as the result.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> as result.</returns>
    IEnumerable<T> Retrieve<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source,
    ///     and returns an <see cref="IEnumerable{T}"/> serialization of the result, as an asynchronous operation.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="altr">The action to modify the generated models within the mapping process.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="IEnumerable{T}"/> as result.
    /// </returns>
    Task<IEnumerable<T>> RetrieveJsonAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source,
    ///     and returns an <see cref="IEnumerable{T}"/> serialization of the result.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> as result.</returns>
    IEnumerable<T> RetrieveJson<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the serialization the first row of
    ///     the result, if any; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the serialization of the first row of the result, if any.
    /// </returns>
    Task<T?> JsonAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the serialization the first row of
    ///     the result, if any; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The serialization of the first row of the result, if any.</returns>
    T? Json<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the first row of
    ///     the result, if any; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the first row of the result, if any.
    /// </returns>
    Task<T> FirstOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the first row of
    ///     the result, if any; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The first row of the result, if any.</returns>
    T FirstOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the result if it only
    ///     a single row; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing an returns the result if it only
    ///     a single row; otherwise, <see langword="null" />
    /// </returns>
    Task<T?> SingleOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement against the data source, and returns the result if it only
    ///     a single row; otherwise, <see langword="null" />.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The result if it only a single row; otherwise, <see langword="null" />.</returns>
    T? SingleOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement againts the data source, and returns scalar value
    ///     on first column of first row in the returned result set, as an asynchronous operation.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, contaning the scalar value on first
    ///     column of first row in the returned result set, if any.
    /// </returns>
    Task<object?> ExecuteScalarAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the specified <paramref name="sql"/> statement againts the data source, and returns scalar value
    ///     on first column of first row in the returned result set.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns> The scalar value on first column of first row in the returned result set.</returns>
    object? ExecuteScalar(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>Invokes <see cref="DbDataReader"/>.</summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>
    ///     The <see cref="Task"/> that represent the asynchronous operation, containing the invoked <see cref="DbDataReader"/>.
    /// </returns>
    Task<DbDataReader> ExecuteReaderAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default);

    /// <summary>Invokes <see cref="DbDataReader"/>.</summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
    /// <returns>The invoked <see cref="DbDataReader"/>.</returns>
    DbDataReader ExecuteReader(string sql, object? prms = null, int? timeout = null, CommandType? type = null);
}
