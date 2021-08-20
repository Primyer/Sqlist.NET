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

using Sqlist.NET.Abstractions;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Sqlist.NET
{
    /// <summary>
    ///     Saves the connection state allowing the reusing of that particular connection
    ///     in a chain of queries. This entity scopes a connection of its own. Any connection or transaciton
    ///     used through an instance of this class is bound to and to be disposed with it.
    ///     <para>
    ///         It's possible to create multiple instance of this class through the <see cref="DbCore"/>,
    ///         where every "persistent" instance is bound to the same scope as the source <see cref="DbCore"/>.
    ///         This allows to use the same connection instance in different places. Thus, a non-persistent
    ///         instance finalizes at the end of the containing block.
    ///     </para>
    ///     <para>
    ///         Note that ADO.NET -being the underlying core for this API- supports connection pooling.
    ///         So, it's recommended to reuse the same connection only when it's needed, since reusing
    ///         the same connection on higher scopes makes it harder to manage asyncronization on the long term.
    ///     </para>
    /// </summary>
    public class DbQuery : QueryStore, IDisposable
    {
        private readonly DbCore _db;
        private readonly bool _persistent;
        private bool _disposed;


        /// <summary>
        ///     Initializes a new instance of the <see cref="DbQuery"/> class.
        /// </summary>
        /// <param name="db">The source <see cref="DbCoreBase"/>.</param>
        internal DbQuery(DbCore db, bool persistent)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
            _persistent = persistent;
        }

        /// <summary>
        ///     Finalizes the current instance.
        /// </summary>
        ~DbQuery()
        {
            Dispose();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Dispose(!_persistent);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether to dispose unmanaged resources.</param>
        internal void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _db.FinalizeQuery(this);

            if (disposing)
            {
                if (_db.Transaction != null)
                {
                    _db.Transaction.Rollback();
                    _db.Transaction.Dispose();
                    _db.Transaction = null;
                }

                _db.Connection.Close();
                _db.Connection.Dispose();
                _db.Connection = null;
            }

            _disposed = true;
        }

        /// <summary>
        ///     Throws an exception if this object was already disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <inheritdoc />
        public override Task<int> ExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            return WrapQuery(() =>
            {
                using var cmd = _db.CreateCommand(_db.Connection, sql, prms, timeout, type);
                return cmd.ExecuteNonQueryAsync();
            });
        }

        /// <inheritdoc />
        public override Task<IEnumerable<T>> RetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            return WrapQuery(async () =>
            {
                using var cmd = _db.CreateCommand(_db.Connection, sql, prms, timeout, type);
                using var reader = await cmd.ExecuteReaderAsync();
                return DataMapper.Parse(reader, altr);
            });
        }
    }
}