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
    public abstract class QueryStore : IQueryStore, IDisposable
    {
        /// <summary>
        ///     Wraps an <see cref="IQueryStore"/> method and returns its result.
        /// </summary>
        /// <typeparam name="T">The type of the query result.</typeparam>
        /// <param name="func">The function to invoke the query.</param>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation, containing the value of the injected function.</returns>
        protected Task<T> WrapQuery<T>(Func<Task<T>> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                Dispose();
                throw ex;
            }
        }

        /// <inheritdoc />
        public abstract void Dispose();

        /// <inheritdoc />
        public virtual int Execute(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return ExecuteAsync(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public abstract Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null);

        /// <inheritdoc />
        public virtual IEnumerable<T> Retrieve<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            return RetrieveAsync(sql, prms, altr, timeout, type).Result;
        }

        /// <inheritdoc />
        public abstract Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null) where T : class, new();

        /// <inheritdoc />
        public virtual async Task<T> FirstOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            if (!result.Any())
                return null;

            return result.First();
        }

        /// <inheritdoc />
        public virtual T FirstOrDefaultc<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            return FirstOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }

        /// <inheritdoc />
        public virtual async Task<T> SingleOrDefaultAsync<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            var result = await RetrieveAsync<T>(sql, prms, null, timeout, type);
            if (result.Count() != 1)
                return null;

            return result.First();
        }

        /// <inheritdoc />
        public virtual T SingleOrDefault<T>(string sql, object prms = null, int? timeout = null, CommandType? type = null) where T : class, new()
        {
            return SingleOrDefaultAsync<T>(sql, prms, timeout, type).Result;
        }
    }
}
