using Sqlist.NET.Infrastructure;
using Sqlist.NET.Serialization;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Sqlist.NET.Abstractions
{
    /// <summary>
    ///     Implementes the <see cref="IQueryStore"/> API.
    /// </summary>
    public abstract class QueryStore : IQueryStore
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
        protected abstract ValueTask<DbConnection> GetConnectionAsync();

        /// <summary>
        ///     Creates and returns a <see cref="Command"/> object.
        /// </summary>
        /// <returns>The <see cref="Command"/> object.</returns>
        public abstract Command CreateCommand();

        /// <summary>
        ///     Creates and returns a <see cref="Command"/> object.
        /// </summary>
        /// <param name="connection">A custom connection, which the command is to be executed over.</param>
        /// <returns>The <see cref="Command"/> object.</returns>
        public abstract Command CreateCommand(DbConnection connection);

        /// <summary>
        ///     Creates and returns a <see cref="Command"/> object.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>The <see cref="Command"/> object.</returns>
        public abstract Command CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null);

        /// <summary>
        ///     Returns an <see cref="Action"/> to be called when the a database command is completed.
        /// </summary>
        /// <returns>An <see cref="Action"/> to be called when the a database command is completed.</returns>
        protected virtual Action OnCommandCompleted()
        {
            return () => { };
        }

        public virtual int Execute(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteAsync(sql, prms, timeout, type).Result;
        }

        public virtual async Task<int> ExecuteAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var cmd = await CreateCommandAsync(sql, prms, timeout, type);
            var task = cmd.ExecuteNonQueryAsync();

            task.GetAwaiter().OnCompleted(OnCommandCompleted());

            return await task;
        }

        public virtual IEnumerable<T> Retrieve<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
        {
            return RetrieveAsync(sql, prms, altr, timeout, type).Result;
        }

        public virtual async Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
        {
            var cmd = await CreateCommandAsync(sql, prms, timeout, type);
            var rdr = cmd.PrepareReader();

            var action = OnCommandCompleted();
            rdr.Fetched += () => action();

            var objType = typeof(T);

            return objType.IsPrimitive || objType.IsValueType || objType.IsArray || objType == typeof(string)
                ? await DataSerializer.Primitive<T>(rdr)
                : await DataSerializer.Object(rdr, _options.MappingOrientation, altr);
        }

        public async Task<IEnumerable<T>> RetrieveJsonAsync<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
        {
            var cmd = await CreateCommandAsync(sql, prms, timeout, type);
            var rdr = cmd.PrepareReader();

            var action = OnCommandCompleted();
            rdr.Fetched += () => action();

            return await DataSerializer.Json<T>(rdr);
        }

        public IEnumerable<T> RetrieveJson<T>(string sql, object? prms = null, Action<T>? altr = null, int? timeout = null, CommandType? type = null)
        {
            return RetrieveJsonAsync(sql, prms, altr, timeout, type).Result;
        }

        public async Task<T?> JsonAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveJsonAsync<T>(sql, prms, null, timeout, type);
            return result.FirstOrDefault();
        }

        public T? Json<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return JsonAsync<T>(sql, prms, timeout, type).Result;
        }

        public virtual T FirstOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return FirstOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        public virtual async Task<T> FirstOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            if (!result.Any())
                return default!;

            return result.First();
        }

        public virtual T? SingleOrDefault<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return SingleOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        public virtual async Task<T?> SingleOrDefaultAsync<T>(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            return result.SingleOrDefault();
        }

        public virtual async Task<object?> ExecuteScalarAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var cmd = await CreateCommandAsync(sql, prms, timeout, type);
            var task = cmd.ExecuteScalarAsync();

            task.GetAwaiter().OnCompleted(OnCommandCompleted());

            return await task;
        }

        public virtual object? ExecuteScalar(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteScalarAsync(sql, prms, timeout, type).Result;
        }

        public virtual async Task<DbDataReader> ExecuteReaderAsync(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            var cmd = await CreateCommandAsync(sql, prms, timeout, type);
            return await cmd.ExecuteReaderAsync();
        }

        public virtual DbDataReader ExecuteReader(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteReaderAsync(sql, prms, timeout, type).Result;
        }

        private async Task<Command> CreateCommandAsync(string sql, object? prms, int? timeout, CommandType? type)
        {
            var cnn = await GetConnectionAsync();
            var cmd = CreateCommand(cnn);

            cmd.Statement = sql;
            cmd.Parameters = prms;
            cmd.Timeout = timeout;
            cmd.Type = type;

            return cmd;
        }
    }
}
