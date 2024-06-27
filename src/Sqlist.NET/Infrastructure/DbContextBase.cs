using Sqlist.NET.Abstractions;
using Sqlist.NET.Data;
using Sqlist.NET.Sql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Infrastructure
{
    /// <summary>
    ///     Provides the basic API to manage a database.
    /// </summary>
    public abstract class DbContextBase : QueryStore, IDisposable, IAsyncDisposable
    {
        private readonly DbDataSource _dataSource;

        private bool _disposed = false;

        private DbConnection? _conn;
        private DbTransaction? _trans;

        private bool _transactionExpected;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextBase"/> class.
        /// </summary>
        /// <param name="factory">Represents a set of methods for creating instances of a provider's implementation</param>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbContextBase(DbOptions options) : base(options)
        {
            Options = options;
            _dataSource = BuildDataSource(options.ConnectionString!);
        }

        public virtual DbConnection? Connection { get => _conn; internal set => _conn = value; }

        /// <summary>
        ///     Gets or sets the pending transaction, if any.
        /// </summary>
        /// <remarks>
        ///     Only applicable with a <see cref="DbQuery"/>.
        /// </remarks>
        public virtual DbTransaction? Transaction { get => _trans; internal set => _trans = value; }

        /// <summary>
        ///     Gets or sets the Sqlist configuration options.
        /// </summary>
        public virtual DbOptions Options { get; }

        /// <summary>
        ///     Gets the default database of the DB provider.
        /// </summary>
        public abstract string? DefaultDatabase { get; }

        /// <summary>
        ///     Gets the <see cref="TypeMapper"/> implementation of the DB provider.
        /// </summary>
        public abstract TypeMapper TypeMapper { get; }

        /// <summary>
        ///     Throws an exception if this object was already disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException" />
        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            DisposeAsync().AsTask().Wait();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public virtual async ValueTask DisposeAsync()
        {
            if (_disposed || _conn is null)
                return;

            if (_trans != null)
                await RollbackTransactionAsync();

            if (_conn != null)
                await _conn.DisposeAsync();

            await _dataSource.DisposeAsync();

            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        ///     Invokes the shared connection, if not doesn't already exit.
        /// </summary>
        /// <param name="revoke">The flag indicating whether to inforce connection revoke.</param>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task InvokeConnectionAsync(bool revoke = false)
        {
            if (revoke && _conn is not null)
                await _conn.DisposeAsync();

            if (revoke || _conn is null)
                _conn = await OpenConnectionAsync();
        }

        /// <inheritdoc />
        protected override ValueTask<DbConnection> GetConnectionAsync()
        {
            return OpenConnectionAsync();
        }

        /// <summary>Invokes a new <see cref="DbConnection"/>.</summary>
        /// <param name="connectionString">A connection string of another data source to connection to.</param>
        /// <returns>
        ///     The <see cref="Task"/> that represents the asynchronous operation, containing the invoked <see cref="DbConnection"/>.
        /// </returns>
        /// <exception cref="DbConnectionException"></exception>
        public ValueTask<DbConnection> OpenConnectionAsync()
        {
            return _dataSource.OpenConnectionAsync();
        }

        public DbConnection CreateConnection()
        {
            return _dataSource.CreateConnection();
        }

        /// <summary>
        ///     Builds and returns <see cref="DbDataSource"/> of the related DB provider.
        /// </summary>
        /// <param name="connectionString">The connection string of the DB provider to be used.</param>
        /// <returns>Returns the <see cref="DbDataSource"/> of the related DB provider.</returns>
        public abstract DbDataSource BuildDataSource(string connectionString);

        /// <summary>
        ///     Returns a new version of the internal connection string with the specified <paramref name="database"/>.
        /// </summary>
        /// <param name="database">The database to modify the internal connection string to.</param>
        /// <returns>A new version of the internal connection string with the specified <paramref name="database"/></returns>
        public abstract string ChangeDatabase(string database);

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

            if (_trans != null)
                throw new DbTransactionException("An ongoing transaction already exists.");

            if (_conn is null)
            {
                _transactionExpected = true;
                return;
            }

            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();

            _trans = await _conn.BeginTransactionAsync();
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
        public virtual async Task CommitTransactionAsync()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be committed.");

            await _trans.CommitAsync();
            _trans = null;
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
        public virtual async Task RollbackTransactionAsync()
        {
            ThrowIfDisposed();

            if (_trans == null)
                throw new DbTransactionException("No transaction to be rolled Wback.");

            await _trans.RollbackAsync();
            _trans = null;
        }

        /// <inheritdoc />
        public override Command CreateCommand()
        {
            return new Command(this);
        }

        /// <inheritdoc />
        public override Command CreateCommand(DbConnection connection)
        {
            return new Command(this, connection);
        }

        /// <inheritdoc />
        public override Command CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
        {
            return new Command(this, sql, prms, timeout, type);
        }

        /// <summary>
        ///     Returns a new instance of the <see cref="IQueryStore"/>.
        /// </summary>
        /// <returns>A new instance of the <see cref="IQueryStore"/>.</returns>
        public virtual IQueryStore Query()
        {
            ThrowIfDisposed();

            var query = new DbQuery(this, _transactionExpected);
            _transactionExpected = false;

            return query;
        }

        /// <summary>
        ///     Invokes and returns the a new case-insensitive <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder UncasedSql() => Sql(new DummyEncloser(), null, null);

        /// <summary>
        ///     Invokes and returns the a new case-insensitive <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <param name="table">The table that's referenced in the statement.</param>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder UncasedSql(string? table) => Sql(new DummyEncloser(), null, table);

        /// <summary>
        ///     Invokes and returns the a new case-insensitive <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <param name="schema">The schema where the sql statement is to be executed.</param>
        /// <param name="table">The table that's referenced in the statement.</param>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder UncasedSql(string? schema, string? table) => Sql(new DummyEncloser(), schema, table);

        /// <summary>
        ///     Invokes and returns the a new <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder Sql() => Sql(null);

        /// <summary>
        ///     Invokes and returns the a new <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <param name="table">The table that's referenced in the statement.</param>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder Sql(string? table) => Sql(null, table);

        /// <summary>
        ///     Invokes and returns the a new <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <param name="schema">The schema where the sql statement is to be executed.</param>
        /// <param name="table">The table that's referenced in the statement.</param>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public SqlBuilder Sql(string? schema, string? table) => Sql(null, schema, table);

        /// <summary>
        ///     Invokes and returns the a new <see cref="SqlBuilder"/> instance.
        /// </summary>
        /// <param name="encloser">The appopertiate <see cref="Sql.Encloser"/> implementation according to the target DBMS.</param>
        /// <param name="schema">The schema where the sql statement is to be executed.</param>
        /// <param name="table">The table that's referenced in the statement.</param>
        /// <returns>The <see cref="SqlBuilder"/>.</returns>
        public abstract SqlBuilder Sql(Encloser? encloser, string? schema, string? table);

        public Task CopyFromAsync(DbConnection source, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
        {
            if (_conn is null)
                throw new DbConnectionException("Destination connection is not initialized.");

            return CopyAsync(source, _conn, table, rules, cancellationToken);
        }

        public Task CopyToAsync(DbConnection destination, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
        {
            if (_conn is null)
                throw new DbConnectionException("Source connection is not initialized.");

            return CopyAsync(_conn, destination, table, rules, cancellationToken);
        }

        public abstract Task CopyAsync(DbConnection exporter, DbConnection importer, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default);

        public abstract Task CopyFromAsync(DbDataReader reader, string table, ICollection<KeyValuePair<string, string>> columns, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Terminates remote connections and clears the connection pool of all related connection instances.
        /// </summary>
        /// <remarks>
        ///     If the currently connected database is the same as the given <paramref name="database"/>, the connection is switched to the provider-default.
        /// </remarks>
        /// <param name="database">The database whose connections are to be terminated.</param>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public abstract Task TerminateDatabaseConnectionsAsync(string database);
    }
}
