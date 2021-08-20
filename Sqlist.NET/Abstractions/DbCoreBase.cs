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

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Data;
using System.Data.Common;

namespace Sqlist.NET.Abstractions
{
    /// <summary>
    ///     Provides the basic API to manage a database.
    /// </summary>
    public abstract class DbCoreBase : QueryStore
    {
        private bool _disposed = false;

        private DbConnection _conn;
        private DbTransaction _trans;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCoreBase"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbCoreBase(DbOptions options)
        {
            Check.NotNull(options, nameof(options));

            Options = options;
        }

        /// <summary>
        ///     Finalizes the current instance.
        /// </summary>
        ~DbCoreBase()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Gets or sets the connection reference in use.
        /// </summary>
        /// <remarks>
        ///     Only applicable with a <see cref="DbQuery"/>.
        /// </remarks>
        public DbConnection Connection
        {
            get => _conn ?? CreateConnection();
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
        public DbTransaction Transaction
        {
            get => _trans;
            internal set
            {
                _trans = value;
            }
        }

        /// <summary>
        ///     Gets or sets the Sqlist configuration options.
        /// </summary>
        public DbOptions Options { get; }

        /// <inheritdoc />
        public override void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting
        ///     unmanaged resources.
        /// </summary>
        /// <param name="disposing">The flag indicating whether this instance is desposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_trans != null)
                {
                    _trans.Rollback();
                    _trans.Dispose();
                }
                if (_conn != null)
                {
                    _conn.Close();
                    _conn.Dispose();
                }
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

        /// <summary>
        ///     Starts a database transaction.
        /// </summary>
        public virtual void BeginTransaction()
        {
            ThrowIfDisposed();

            if (_conn == null)
                throw new InvalidOperationException("A transaction can only be applied within a DbQuery");

            _trans = _conn.BeginTransaction();
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
        ///     Creates and returns a new instance of the configured provider's class
        ///     that implements the <see cref="DbCommand"/> class.
        /// </summary>
        /// <param name="conn">The <see cref="DbConnection"/> to initialize the command from.</param>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>A new instance of <see cref="DbCommand"/>.</returns>
        internal virtual DbCommand CreateCommand(DbConnection conn, string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            ThrowIfDisposed();
            Check.NotNullOrEmpty(sql, nameof(sql));

            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = type ?? CommandType.Text;
            cmd.Transaction = _trans;

            if (timeout.HasValue)
                cmd.CommandTimeout = timeout.Value;

            if (prms != null)
                ConfigureParameters(cmd, prms);

            return cmd;
        }

        /// <summary>
        ///     Creates and returns a new instance of the configured provider's class
        ///     that implements the <see cref="DbCommand"/> class.
        /// </summary>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        /// <returns>A new instance of <see cref="DbCommand"/>.</returns>
        public virtual DbCommand CreateCommand(string sql, object prms = null, int? timeout = null, CommandType? type = null)
        {
            return CreateCommand(CreateConnection(), sql, prms, timeout, type);
        }

        /// <summary>
        ///     Configure the specified <paramref name="prms"/> to be added to the given <paramref name="cmd"/> later on.
        /// </summary>
        /// <param name="cmd">The <see cref="DbCommand"/> that owns the parameters.</param>
        /// <param name="prms">The anonymous object representing the parameters.</param>
        public virtual void ConfigureParameters(DbCommand cmd, object prms)
        {
            ThrowIfDisposed();

            foreach (var prop in prms.GetType().GetProperties())
            {
                var prm = cmd.CreateParameter();

                prm.ParameterName = prop.Name;
                prm.Direction = ParameterDirection.Input;
                prm.DbType = TypeMapper.Instance.ToDbType(prop.PropertyType);
                prm.Value = prop.GetValue(prms);

                cmd.Parameters.Add(prm);
            }
        }

        /// <summary>
        ///     Creates a new instance of the configured provider's class that implements the <see cref="DbConnection"/> class.
        /// </summary>
        /// <returns>A new instance of <see cref="DbConnection"/>.</returns>
        public virtual DbConnection CreateConnection()
        {
            ThrowIfDisposed();

            DbConnection conn = null;

            // Ensure the existense of the required information..
            if (Options.DbProviderFactory == null || Options.ConnectionString == null)
                throw new DbConnectionException("Could not create a connection string. The options are not properly configured.");
            
            try
            {
                conn = Options.DbProviderFactory.CreateConnection();
                conn.ConnectionString = Options.ConnectionString;
            }
            catch (Exception ex)
            {
                if (conn != null)
                    throw new DbConnectionException("The database connection was created, but failed later on.", ex);

                throw ex;
            }
            return conn;
        }
    }
}
