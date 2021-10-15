﻿#region License
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
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

using ado = System.Data.Common;
using lcl = Sqlist.NET.Common;

namespace Sqlist.NET
{
    /// <summary>
    ///     Provides the basic API to manage a database.
    /// </summary>
    public class DbCore : QueryStore
    {
        private readonly List<Guid> _queries = new List<Guid>();

        private bool _disposed = false;

        private lcl::DbConnection _conn;
        private ado::DbTransaction _trans;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCore"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbCore(DbOptions options, ILogger<DbCore> log)
        {
            Check.NotNull(options, nameof(options));
            Check.NotNull(log, nameof(log));

            Options = options;
            Logger = log;

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
        public lcl::DbConnection Connection
        {
            get => _conn ??= lcl::DbConnection.CreateFor(this);
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

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public virtual void BeginTransaction()
        {
            ThrowIfDisposed();

            if (_conn == null)
                throw new InvalidOperationException("A transaction can only be applied within a DbQuery");

            _trans = _conn.Underlying.BeginTransaction();
        }

        /// <summary>
        ///     Commits the database transaction.
        /// </summary>
        public virtual void CommitTransaction()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be committed.");

            _trans.Commit();
        }

        /// <summary>
        ///     Rolls back a transaction from a pending state.
        /// </summary>
        public virtual void RollbackTransaction()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be rolled back.");

            _trans.Rollback();
        }

        /// <summary>
        ///     Returns a new instance of the <see cref="IQueryStore"/>.
        /// </summary>
        /// <returns>A new instance of the <see cref="IQueryStore"/>.</returns>
        public virtual IQueryStore Query(bool terminator = false)
        {
            ThrowIfDisposed();

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

            _queries.Remove(qry.Id);

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
        protected override async Task<T> ExecuteQueryAsync<T>(string name, Func<Task<T>> func)
        {
            Logger.LogTrace("Executing independent query ({name}) over conn[{connId}]", name, Connection.Id);

            try
            {
                var sw = Stopwatch.StartNew();
                var result = await func.Invoke();
                sw.Stop();

                Logger.LogTrace("Query ({name}) succeeded. Elapsed time: {time}ms", name, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError("Query ({name}) failed", name);
                throw ex;
            }
        }

        protected internal override Task<object> InternalExecuteScalarAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            var conn = lcl::DbConnection.CreateFor(this);
            try
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                return cmd.ExecuteScalarAsync();
            }
            finally
            {
                conn.Dispose();
            }
        }

        /// <inheritdoc />
        protected internal override Task<int> InternalExecuteAsync(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            var conn = lcl::DbConnection.CreateFor(this);
            try
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                return cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                conn.Dispose();
            }
        }

        /// <inheritdoc />
        protected internal override Task<IEnumerable<T>> InternalRetrieveAsync<T>(string sql, object prms = null, Action<T> altr = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();

            var conn = lcl::DbConnection.CreateFor(this);
            try
            {
                var cmd = conn.CreateCommand(sql, prms, timeout, type);
                var rdr = cmd.ExecuteReaderAsync();

                var objType = typeof(T);
                var result = objType.IsPrimitive || objType.IsValueType || objType.IsArray || objType == typeof(string)
                    ? DataParser.Primitive<T>(rdr)
                    : DataParser.Object(rdr, Options.MappingOrientation, altr);

                return result;
            }
            finally
            {
                conn.Dispose();
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
