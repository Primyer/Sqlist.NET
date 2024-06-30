using Sqlist.NET.Serialization;
using Sqlist.NET.Utilities;

using System.Data;
using System.Data.Common;

namespace Sqlist.NET.Infrastructure;

public delegate void CommandCompletedEvent();

/// <summary>
///     Implementes the <see cref="IQueryStore"/> API.
/// </summary>
public abstract class QueryStore : IQueryStore, ICommandProvider
{
    private readonly DbOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryStore"/> class.
    /// </summary>
    public QueryStore(DbOptions options)
    {
        Check.NotNull(options);
        _options = options;
    }

    /// <summary>
    ///     Gets the database connection.
    /// </summary>
    /// <returns>The database connection.</returns>
    internal protected abstract ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates and returns a <see cref="Command"/> object.
    /// </summary>
    /// <returns>The <see cref="Command"/> object.</returns>
    public abstract ICommand CreateCommand();

    /// <summary>
    ///     Creates and returns a <see cref="Command"/> object.
    /// </summary>
    /// <param name="connection">A custom connection, which the command is to be executed over.</param>
    /// <returns>The <see cref="Command"/> object.</returns>
    public abstract ICommand CreateCommand(DbConnection connection);

    /// <summary>
    ///     Creates and returns a <see cref="Command"/> object.
    /// </summary>
    /// <param name="sql">The SQL statement to run against the data source.</param>
    /// <param name="prms">The parameters associated with the given statement.</param>
    /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
    /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
    /// <returns>The <see cref="Command"/> object.</returns>
    public abstract ICommand CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

    /// <summary>
    ///     Gets or sets the <see cref="CommandCompletedEvent"/> to be invoked when the a database command is completed.
    /// </summary>
    internal protected event CommandCompletedEvent OnCompleted = () => { };

    private TResult NotifyCommandCompleted<TResult>(Task<TResult> task)
    {
        NotifyCommandCompleted();
        return task.Result;
    }

    private void NotifyCommandCompleted()
    {
        OnCompleted.Invoke();
    }

    public int Execute(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return ExecuteAsync(sql, prms, timeout, type).Result;
    }

    public virtual async Task<int> ExecuteAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var cmd = await CreateCommandAsync(sql, prms, timeout, type, cancellationToken);
        var task = cmd.ExecuteNonQueryAsync(cancellationToken);

        return await task.ContinueWith(NotifyCommandCompleted);
    }

    public IEnumerable<T> Retrieve<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
    {
        return RetrieveAsync(sql, prms, altr, timeout, type).Result;
    }

    public virtual async Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var cmd = await CreateCommandAsync(sql, prms, timeout, type, cancellationToken);
        var rdr = cmd.PrepareReader(cancellationToken: cancellationToken);

        rdr.Fetched += NotifyCommandCompleted;

        var objType = typeof(T);

        return objType.IsPrimitive || objType.IsValueType || objType.IsArray || objType == typeof(string)
            ? await DataSerializer.Primitive<T>(rdr)
            : await DataSerializer.Object(rdr, _options.MappingOrientation, altr);
    }

    public virtual async Task<IEnumerable<T>> RetrieveJsonAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var cmd = await CreateCommandAsync(sql, prms, timeout, type, cancellationToken);
        var rdr = cmd.PrepareReader(cancellationToken: cancellationToken);

        rdr.Fetched += NotifyCommandCompleted;

        return await DataSerializer.Json<T>(rdr);
    }

    public IEnumerable<T> RetrieveJson<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
    {
        return RetrieveJsonAsync(sql, prms, altr, timeout, type).Result;
    }

    public virtual async Task<T?> JsonAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var result = await RetrieveJsonAsync<T>(sql, prms, null, timeout, type, cancellationToken);
        return result.FirstOrDefault();
    }

    public T? Json<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return JsonAsync<T>(sql, prms, timeout, type).Result;
    }

    public T FirstOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return FirstOrDefaultAsync<T>(sql, prms, timeout, type).Result;
    }

    public virtual async Task<T> FirstOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var result = await RetrieveAsync<T>(sql, prms, null, timeout, type, cancellationToken);
        if (!result.Any())
            return default!;

        return result.First();
    }

    public T? SingleOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return SingleOrDefaultAsync<T>(sql, prms, timeout, type).Result;
    }

    public virtual async Task<T?> SingleOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var result = await RetrieveAsync<T>(sql, prms, null, timeout, type, cancellationToken);
        return result.SingleOrDefault();
    }

    public virtual async Task<object?> ExecuteScalarAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var cmd = await CreateCommandAsync(sql, prms, timeout, type, cancellationToken);
        var task = cmd.ExecuteScalarAsync(cancellationToken);

        return await task.ContinueWith(NotifyCommandCompleted);
    }

    public object? ExecuteScalar(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return ExecuteScalarAsync(sql, prms, timeout, type).Result;
    }

    public virtual async Task<DbDataReader> ExecuteReaderAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null, CancellationToken cancellationToken = default)
    {
        var cmd = await CreateCommandAsync(sql, prms, timeout, type, cancellationToken);
        return await cmd.ExecuteReaderAsync(cancellationToken: cancellationToken);
    }

    public DbDataReader ExecuteReader(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return ExecuteReaderAsync(sql, prms, timeout, type).Result;
    }

    private async Task<ICommand> CreateCommandAsync(string sql, object? prms, int? timeout, CommandType? type, CancellationToken cancellationToken = default)
    {
        var cnn = await GetConnectionAsync(cancellationToken);
        var cmd = CreateCommand(cnn);

        cmd.Statement = sql;
        cmd.Parameters = prms;
        cmd.Timeout = timeout;
        cmd.Type = type;

        return cmd;
    }
}