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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sqlist.NET.Abstractions
{
    /// <summary>
    ///     Implementes the <see cref="IQueryStore"/> API.
    /// </summary>
    public abstract class QueryStore : IQueryStore
    {
        /// <summary>
        ///     Wraps an <see cref="IQueryStore"/> method and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the query result.</typeparam>
        /// <param name="name">The name of the executing method.</param>
        /// <param name="func">The function to invoke the query.</param>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation, containing the value of the injected function.</returns>
        protected abstract Task<T> ExecuteQueryAsync<T>(string name, Func<Task<T>> func);

        /// <inheritdoc />
        public virtual int Execute(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return ExecuteQueryAsync(nameof(Execute), () =>
                InternalExecuteAsync(sql, prms, timeout, type)).Result;
#else
            return InternalExecuteAsync(sql, prms, timeout, type).Result;
#endif
        }

        /// <inheritdoc />
        public virtual Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return ExecuteQueryAsync(nameof(ExecuteAsync), () =>
            {
#endif
                return InternalExecuteAsync(sql, prms, timeout, type);
#if TRACE
            });
#endif
        }

        /// <inheritdoc />
        public virtual IEnumerable<T> Retrieve<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            return RetrieveAsync(sql, prms, altr, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return ExecuteQueryAsync(nameof(RetrieveAsync), () =>
            {
#endif
                return InternalRetrieveAsync(sql, prms, altr, timeout, type);
#if TRACE
            });
#endif
        }

        /// <inheritdoc />
        public virtual T FirstOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return FirstOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual async Task<T> FirstOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return await ExecuteQueryAsync(nameof(FirstOrDefaultAsync), async () =>
            {
#endif
                var result = await InternalRetrieveAsync<T>(sql, prms, null, timeout, type);
                if (!result.Any())
                    return default;

                return result.First();
#if TRACE
            });
#endif
        }

        /// <inheritdoc />
        public virtual T SingleOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return SingleOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual async Task<T> SingleOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return await ExecuteQueryAsync(nameof(SingleOrDefaultAsync), async () =>
            {
#endif
                var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
                return result.SingleOrDefault();
#if TRACE
            });
#endif
        }

        /// <inheritdoc />
        public virtual Task<object> ExecuteScalarAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
#if TRACE
            return ExecuteQueryAsync(nameof(ExecuteScalarAsync), () =>
            {
#endif
                return InternalExecuteScalarAsync(sql, prms, timeout, type);
#if TRACE
            });
#endif
        }

        /// <inheritdoc />
        public virtual object ExecuteScalar(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteScalarAsync(sql, prms, timeout, type).Result;
        }

        protected internal abstract Task<object> InternalExecuteScalarAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        protected internal abstract Task<int> InternalExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        protected internal abstract Task<IEnumerable<T>> InternalRetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null);

        /// <inheritdoc />
        public abstract void Dispose();
    }
}
