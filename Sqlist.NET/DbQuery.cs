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

using Sqlist.NET.Abstractions;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sqlist.NET
{
    /// <summary>
    ///     <see cref="DbQuery"/> allows reusing a single database connection. Nevertheless, it allows
    ///     the same connection to be reused by any <see cref="DbQuery"/> nested in between the initialization
    ///     and the release of the highest <see cref="DbQuery"/> instance.
    /// </summary>
    /// <remarks>
    ///     The connection in this case is never closed until the heighest query is closed. Then, the connection
    ///     is to be closed and returned to the connection pool to be reused later. This connection keeps
    ///     alive along the lifetime of the containing <see cref="DbCore"/> instance and to be disposed with it.
    /// </remarks>
    public class DbQuery : QueryStore
    {
        private readonly DbCore _db;

        private bool _disposed;
        private int _delayQueueLength = 0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbQuery"/> class.
        /// </summary>
        /// <param name="db">The source <see cref="DbCore"/>.</param>
        internal DbQuery(DbCore db)
        {
            Check.NotNull(db, nameof(db));

            _db = db;

            Id = Guid.NewGuid();
#if TRACE
            db.Logger.LogTrace("Query prepared over conn:[" + db.Connection.Id + "]. ID: [" + Id + "]");
#endif
        }

        /// <summary>
        ///     Gets the ID of this query.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Gets or sets the flag indicating whether to terminate the connection at the end
        ///     of this query.
        /// </summary>
        public bool TerminateConnection { get; set; }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (!_disposed && _delayQueueLength == 0)
            {
                _db.FinalizeQuery(this);
                _disposed = true;
#if TRACE
                _db.Logger.LogTrace("Query released. ID: [" + Id + "]");
#endif
            }
        }

        /// <summary>
        ///     Throws an exception if this object was already disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DbQuery));
        }

        /// <inheritdoc />
        protected override async Task<T> ExecuteQueryAsync<T>(string name, Func<Task<T>> func)
        {
            _db.Logger.LogTrace("Executing query[{id}] ({name}) over conn[{connId}]", Id, name, _db.Connection.Id);

            try
            {
                var sw = Stopwatch.StartNew();
                var result = await func.Invoke();
                sw.Stop();

                _db.Logger.LogTrace("Query[{id}] ({name}) succeeded. Elapsed time: {time}ms", Id, name, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                _db.Logger.LogError("Query[{id}] ({name}) failed", Id, name);
                throw ex;
            }
        }

        internal Action RegisterDelayer()
        {
            _delayQueueLength++;

            return () =>
            {
                if (--_delayQueueLength == 0)
                    Dispose();
            };
        }

        protected internal override Task<int> InternalExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            var cmd = _db.Connection.CreateCommand(sql, prms, timeout, type);
            return cmd.ExecuteNonQueryAsync();
        }

        protected internal override Task<IEnumerable<T>> InternalRetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            var cmd = _db.Connection.CreateCommand(sql, prms, timeout);
            var rdr = cmd.ExecuteReaderAsync();

            var action = RegisterDelayer();
            rdr.Fetched += () => action();

            var objType = typeof(T);
            var result = !objType.IsClass || objType.IsArray
                ? DataParser.Primitive<T>(rdr)
                : DataParser.Object(rdr, _db.Options.MappingOrientation, altr);

            return result;
        }
    }
}