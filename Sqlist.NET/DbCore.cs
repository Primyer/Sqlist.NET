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
using Sqlist.NET.Common;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using ado = System.Data.Common;

namespace Sqlist.NET
{
    /// <summary>
    ///     Provides the basic API to manage a database.
    /// </summary>
    public class DbCore : QueryStore
    {
        private readonly List<Guid> _queries = new List<Guid>();

        private bool _disposed = false;

        private DbConnection _conn;
        private ado::DbTransaction _trans;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCore"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbCore(DbOptions options, ILogger<DbCore> logger) : base(options, logger)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(logger, nameof(logger));

            Options = options;
            Logger = logger;

            Id = Guid.NewGuid();
            Logger.LogDebug("DB created. ID:[" + Id + "]");
        }

        /// <summary>
        ///     The ID of this DB.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        ///     Gets or sets the connection reference in use.
        /// </summary>
        /// <remarks>
        ///     Only applicable with a <see cref="DbQuery"/>.
        /// </remarks>
        public DbConnection Connection
        {
            get => _conn ??= DbConnection.CreateFor(this);
            internal set
            {
                _conn = value;
            }
        }

        /// <summary>
        ///     Gets or sets the pending transaction, if any.
        /// </summary>
        /// <remarks>
        ///     Only applicable with a <see cref="DbQuery"/>.
        /// </remarks>
        public ado::DbTransaction Transaction
        {
            get => _trans;
            internal set
            {
                _trans = value;
            }
        }

        /// <summary>
        ///     Gets the logger used by this instance.
        /// </summary>
        internal ILogger Logger { get; }

        /// <summary>
        ///     Gets or sets the Sqlist configuration options.
        /// </summary>
        public DbOptions Options { get; }

        /// <summary>
        ///     Gets the currently active queries.
        /// </summary>
        public IEnumerable<Guid> Queries => _queries;

        /// <summary>
        ///     Throws an exception if this object was already disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected override DbConnection GetConnection()
        {
            return DbConnection.CreateFor(this);
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public virtual void BeginTransaction()
        {
            BeginTransactionAsync().Wait();
        }

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public virtual async Task BeginTransactionAsync()
        {
            ThrowIfDisposed();

            if (_conn == null)
                throw new InvalidOperationException("A transaction can only be applied within a DbQuery");

            if (_conn.Underlying.State != ConnectionState.Open)
                await _conn.Underlying.OpenAsync();

            _trans = await _conn.Underlying.BeginTransactionAsync();
        }

        /// <summary>
        ///     Commits the database transaction.
        /// </summary>
        public virtual void CommitTransaction()
        {
            CommitTransactionAsync().Wait();
        }

        /// <summary>
        ///     Commits the database transaction.
        /// </summary>
        public virtual Task CommitTransactionAsync()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be committed.");

            return _trans.CommitAsync();
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public virtual void RollbackTransaction()
        {
            RollbackTransactionAsync().Wait();
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public virtual Task RollbackTransactionAsync()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be rolled Wback.");

            return _trans.RollbackAsync();
        }

        /// <summary>
        ///     Returns a new instance of the <see cref="IQueryStore"/>.
        /// </summary>
        /// <returns>A new instance of the <see cref="IQueryStore"/>.</returns>
        public virtual IQueryStore Query(bool terminator = false)
        {
            ThrowIfDisposed();

            _conn ??= DbConnection.CreateFor(this);

            var query = new DbQuery(this);
            _queries.Add(query.Id);

            return query;
        }

        /// <summary>
        ///     Finalizes the given <paramref name="qry"/>.
        /// </summary>
        /// <param name="qry">The <see cref="DbQuery"/> to finalize.</param>
        internal virtual void FinalizeQuery(DbQuery qry)
        {
            ThrowIfDisposed();

            _queries.RemoveAll(guid => guid == qry.Id);

            if (_queries.Count == 0)
            {
                if (!qry.TerminateConnection)
                    Connection.Underlying.Close();
                else
                {
                    Connection.Dispose();
                    Connection = null;
                }
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Finalizes the current instance.
        /// </summary>
        ~DbCore()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <param name="disposing">The flag indicating whether this instance is desposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _conn?.Dispose();
                _disposed = true;

                Logger.LogDebug("DB released. ID:[" + Id + "]");
            }
        }
    }
}
