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

using Sqlist.NET.Utilities;

using System;
using System.Data;
using System.Threading.Tasks;

using ado = System.Data.Common;

namespace Sqlist.NET.Common
{
    public class DbConnection : IDisposable, IAsyncDisposable
    {
        private readonly DbCore _db;
        private readonly ado::DbConnection _conn;

        private bool _disposed;

        /// <summary>
        ///     Initailizes a new instance of the <see cref="DbConnection"/> class.
        /// </summary>
        public DbConnection(DbCore db, ado::DbConnection conn)
        {
            Check.NotNull(db, nameof(db));
            Check.NotNull(conn, nameof(conn));

            _db = db;
            _conn = conn;

#if TRACE
            _conn.StateChange += StateChanged;
#endif
        }

        /// <summary>
        ///     Gets the ID of this connection.
        /// </summary>
        /// <remarks>
        ///     This is not the actual ID of the underlying <see cref="ado::DbConnection"/>.
        ///     It's only used for tracing purposes.
        /// </remarks>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        ///     Gets the containing <see cref="DbCore"/>.
        /// </summary>
        public DbCore DB => _db;

        /// <summary>
        ///     Gets the underlying instance of the <see cref="ado::DbConnection"/>.
        /// </summary>
        public ado::DbConnection Underlying => _conn;

        /// <summary>
        ///     Creates a new instance of the configured provider's class that implements the <see cref="ado::DbConnection"/> class.
        /// </summary>
        /// <param name="db">The containing <see cref="DbCore"/>.</param>
        /// <returns>A new instance of <see cref="ado::DbConnection"/>.</returns>
        public static DbConnection CreateFor(DbCore db)
        {
            // Ensure the existense of the required information..
            if (db.Options.DbProviderFactory == null || db.Options.ConnectionString == null)
                throw new DbConnectionException("Could not create a connection string. The options are not properly configured.");

            ado::DbConnection conn = null;
            try
            {
                conn = db.Options.DbProviderFactory.CreateConnection();
                conn.ConnectionString = db.Options.ConnectionString;
            }
            catch (Exception ex)
            {
                if (conn is null)
                    throw ex;

                conn.Dispose();
                throw new DbConnectionException("The database connection was created, but failed later on.", ex);
            }

            var wrpr = new DbConnection(db, conn);
            db.Logger.LogDebug("Connection created for DB:[" + db.Id + "]. ID:[" + wrpr.Id + "]");
            return wrpr;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <param name="disposing">The flag indicating whether this instance is desposing.</param>
        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                await _conn.CloseAsync(); // Recommended for some data sources. e.g: Oracle DB.
                await _conn.DisposeAsync();
                _disposed = true;

                _db.Logger.LogDebug("Connection released. ID:[" + Id + "]");
            }
        }

        /// <summary>
        ///     Finalizes the current instance.
        /// </summary>
        ~DbConnection()
        {
            DisposeAsync(disposing: false).Wait();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            DisposeAsync(disposing: true).Wait();
            GC.SuppressFinalize(this);
        }

#if TRACE
        private void StateChanged(object source, StateChangeEventArgs args)
        {
            _db.Logger.LogTrace("Connection is " + args.CurrentState + ". ID:[" + Id + "]");
        }
#endif
    }
}
