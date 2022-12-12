#region License
// Copyright (c) 2021, Saleh Kawaf Kulla
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.Extensions.Logging;

using Sqlist.NET.Common;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Serialization;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
        private readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryStore"/> class.
        /// </summary>
        public QueryStore(DbOptions options, ILogger logger)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(logger, nameof(logger));

            _options = options;
            _logger = logger;
        }

        /// <summary>
        ///     Wraps an <see cref="IQueryStore"/> method and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the query result.</typeparam>
        /// <param name="name">The name of the executing method.</param>
        /// <param name="func">The function to invoke the query.</param>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation, containing the value of the injected function.</returns>
        protected async Task<T> ExecuteQueryAsync<T>(string name, Func<DbConnection, Task<T>> func)
        {
            try
            {
                var conn = GetConnection();
                _logger.LogTrace("Executing query ({name}) over conn[{connId}]", name, conn.Id);

                var sw = Stopwatch.StartNew();
                var result = await func.Invoke(conn);
                sw.Stop();

                _logger.LogTrace("Query ({name}) succeeded. Elapsed time: {time}ms", name, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Query ({name}) failed", name);
                throw ex;
            }
        }

        /// <summary>
        ///     Gets the database connection.
        /// </summary>
        /// <returns>The database connection.</returns>
        protected abstract DbConnection GetConnection();

        /// <summary>
        ///     Returns an <see cref="Action"/> to be called when the a database command is completed.
        /// </summary>
        /// <returns>An <see cref="Action"/> to be called when the a database command is completed.</returns>
        protected virtual Action OnCommandCompleted()
        {
            return () => { };
        }

        /// <inheritdoc />
        public virtual int Execute(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteAsync(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteQueryAsync(nameof(ExecuteAsync), conn =>
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                var task = cmd.ExecuteNonQueryAsync();

                task.GetAwaiter().OnCompleted(OnCommandCompleted());

                return task;
            });
        }

        /// <inheritdoc />
        public virtual IEnumerable<T> Retrieve<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            return RetrieveAsync(sql, prms, altr, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteQueryAsync(nameof(RetrieveAsync), conn =>
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                var rdr = cmd.ExecuteReaderAsync();

                var action = OnCommandCompleted();
                rdr.Fetched += () => action();

                var objType = typeof(T);

                return objType.IsPrimitive || objType.IsValueType || objType.IsArray || objType == typeof(string)
                    ? DataSerializer.Primitive<T>(rdr)
                    : DataSerializer.Object(rdr, _options.MappingOrientation, altr);
            });
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> RetrieveJsonAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteQueryAsync(nameof(RetrieveAsync), conn =>
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                var rdr = cmd.ExecuteReaderAsync();

                var action = OnCommandCompleted();
                rdr.Fetched += () => action();

                return DataSerializer.Json<T>(rdr);
            });
        }

        /// <inheritdoc />
        public IEnumerable<T> RetrieveJson<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            return RetrieveJsonAsync(sql, prms, altr, timeout, type).Result;
        }

        public async Task<T> JsonAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveJsonAsync<T>(sql, prms, null, timeout, type);
            return result.FirstOrDefault();
        }

        public T Json<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return JsonAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual T FirstOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return FirstOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual async Task<T> FirstOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            if (!result.Any())
                return default;

            return result.First();
        }

        /// <inheritdoc />
        public virtual T SingleOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return SingleOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual async Task<T> SingleOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            return result.SingleOrDefault();
        }

        /// <inheritdoc />
        public virtual Task<object> ExecuteScalarAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteQueryAsync(nameof(ExecuteScalarAsync), conn =>
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                var task = cmd.ExecuteScalarAsync();

                task.GetAwaiter().OnCompleted(OnCommandCompleted());

                return task;
            });
        }

        /// <inheritdoc />
        public virtual object ExecuteScalar(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteScalarAsync(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public abstract void Dispose();
    }
}
