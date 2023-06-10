using Microsoft.Extensions.Configuration;

using Sqlist.NET.Abstractions;
using Sqlist.NET.Data;
using Sqlist.NET.Sql;
using Sqlist.NET.Utilities;

using System;
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
        private readonly DbProviderFactory _factory;

        private bool _disposed = false;

        private DbConnection? _conn;
        private DbTransaction? _trans;

        private bool _transactionExpected;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextBase"/> class.
        /// </summary>
        /// <param name="factory">Represents a set of methods for creating instances of a provider's implementation</param>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbContextBase(DbProviderFactory factory, DbOptions options) : base(options)
        {
            Check.NotNull(factory, nameof(factory));

            Options = options;
            _factory = factory;
        }

        public DbConnection? Connection
        {
            get => _conn;
            internal set => _conn = value;
        }

        /// <summary>
        ///     Gets or sets the pending transaction, if any.
        /// </summary>
        /// <remarks>
        ///     Only applicable with a <see cref="DbQuery"/>.
        /// </remarks>
        public DbTransaction? Transaction
        {
            get => _trans;
            internal set => _trans = value;
        }

        /// <summary>
        ///     Gets or sets the Sqlist configuration options.
        /// </summary>
        public DbOptions Options { get; }

        /// <summary>
        ///     Gets the connection string.
        /// </summary>
        protected abstract string ConnectionString { get; }

        /// <summary>
        ///     Gets the default database of the DB provider.
        /// </summary>
        public abstract string? DefaultDatabase { get; }

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
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_disposed || _conn is null)
                return;

            if (_trans != null)
                await RollbackTransactionAsync();

            if (_conn != null)
                await _conn.DisposeAsync();

            _disposed = true;
        }

        /// <summary>
        ///     Invokes the shared connection, if not doesn't already exit.
        /// </summary>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task InvokeConnectionAsync()
        {
            _conn ??= await CreateConnectionAsync();
        }

        /// <inheritdoc />
        protected override Task<DbConnection> GetConnectionAsync()
        {
            return CreateConnectionAsync();
        }

        /// <summary>Invokes a new <see cref="DbConnection"/>.</summary>
        /// <param name="connectionString">A connection string of another data source to connection to.</param>
        /// <returns>
        ///     The <see cref="Task"/> that represents the asynchronous operation, containing the invoked <see cref="DbConnection"/>.
        /// </returns>
        /// <exception cref="DbConnectionException"></exception>
        public async Task<DbConnection> CreateConnectionAsync(string? connectionString = null)
        {
            DbConnection? conn = null;
            try
            {
                conn = _factory.CreateConnection();
                conn.ConnectionString = connectionString ?? ConnectionString;
            }
            catch (Exception ex)
            {
                if (conn is null) throw;

                await conn.DisposeAsync();
                throw new DbConnectionException("The database connection was created, but failed later on.", ex);
            }

            await conn.OpenAsync();
            return conn;
        }

        /// <summary>
        ///     Re invokes a new connection to the specified <paramref name="database"/>, closing the current one.
        /// </summary>
        /// <param name="database">The database to connection to.</param>
        /// <returns>The <see cref="Task"/> object that represent the asynchronous operation.</returns>
        public async Task ConnectToDatabaseAsync(string database)
        {
            if (_conn != null)
                await _conn.DisposeAsync();

            var connStr = ChangeDatabase(database);
            _conn = await CreateConnectionAsync(connStr);
        }

        /// <summary>
        ///     Changes the database specified within the connection string, returning the new version.
        /// </summary>
        /// <param name="database">The database to set the connection string to.</param>
        /// <returns>The new version of the connection string with the specified <paramref name="database"/>.</returns>
        protected abstract string ChangeDatabase(string database);

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

        public abstract Task CopyAsync(DbConnection source, DbConnection destination, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default);

        public abstract void TerminateDatabaseConnections(string database);
    }


    /// <summary>
    ///     Provides the basic API to manage a database.
    /// </summary>
    public abstract class DbContextBase<TConnectionStringBuilder> : DbContextBase where TConnectionStringBuilder : DbConnectionStringBuilder, new()
    {
        /// <param name="connectionString">The connection string configuration action.</param>
        /// <inheritdoc />
        public DbContextBase(DbProviderFactory factory, DbOptions options, Action<TConnectionStringBuilder> connectionString) : base(factory, options)
        {
            ConnectionStringBuilder = new TConnectionStringBuilder();
            connectionString(ConnectionStringBuilder);
        }

        /// <inheritdoc />
        protected DbContextBase(DbProviderFactory factory, DbOptions options) : base(factory, options)
        {
            if (options.ConnectionString is null)
                throw new DbException("Connection string configuration cannot be null.");

            ConnectionStringBuilder = options.ConnectionString.Get<TConnectionStringBuilder>() ?? throw new DbException("Invalid connection string configuration.");
        }

        /// <inheritdoc />
        protected override string ConnectionString => ConnectionStringBuilder.ToString();

        /// <summary>
        ///     Gets the <see cref="DbConnectionStringBuilder"/> implementation of the related DB provider.
        /// </summary>
        protected TConnectionStringBuilder ConnectionStringBuilder { get; set; }
    }
}
