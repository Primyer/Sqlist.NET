using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Sqlist.NET
{
    /// <summary>
    ///     Implements the <see cref="DbCoreBase"/> as a base of the provided querying API.
    /// </summary>
    public class DbCore : DbCoreBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCore"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbCore(DbOptions options) : base(options)
        { }

        /// <summary>
        ///     Saves the connection state allowing the reusing of that particular connection
        ///     in a chain of queries. 
        ///     <para>
        ///         Note that ADO.NET -being the underlying core for this API- supports connection pooling.
        ///         So, it's recommended to reuse the same connection only when it's needed, since reusing
        ///         the same connection on higher scopes makes it harder to manage asyncronization on the long term.
        ///     </para>
        /// </summary>
        /// <param name="query">The action to use the shared connection instance.</param>
        public void ReuseConnection(Action<DbCore> query)
        {
            ThrowIfDisposed();
            Check.NotNull(query, nameof(query));

            Connection = CreateConnection();

            query(this);

            Connection.Dispose();
            Connection = null;
        }

        /// <summary>
        ///     Executes the specified <paramref name="sql"/> statement against the data source.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int Execute(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            using var cmd = CreateCommand(sql, prms, timeout, type);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source,
        ///     and returns a <see cref="DbDataReader"/> as the result.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>A <see cref="DbDataReader"/> as result.</returns>
        public virtual DbDataReader ExecuteReader(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            using var cmd = CreateCommand(sql, prms, timeout);
            return cmd.ExecuteReader();
        }

        /// <summary>
        ///     executes the specified <paramref name="sql"/> statement against the data source,
        ///     and returns an <see cref="IEnumerable{T}"/> as the result.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> as result.</returns>
        public virtual IEnumerable<T> Retrieve<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            var reader = ExecuteReader(sql, prms, timeout);
            var result = DataMapper<T>.Parse(reader);

            return result;
        }
    }
}
