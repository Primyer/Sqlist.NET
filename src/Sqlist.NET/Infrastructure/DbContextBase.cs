using System.Data;
using System.Data.Common;

namespace Sqlist.NET.Infrastructure;

public abstract class DbContextBase : QueryStore, IDbContext
{
    private readonly DbDataSource _dataSource;

    private bool _disposed = false;

    private DbConnection? _conn;
    private DbTransaction? _trans;

    private bool _transactionExpected;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextBase"/> class.
    /// </summary>
    /// <param name="options">The Sqlist configuration options.</param>
    public DbContextBase(DbOptions options) : base(options)
    {
        Options = options;
        _dataSource = BuildDataSource(options.ConnectionString!);
    }

    public virtual DbConnection Connection
    {
        get => _conn ?? throw new InvalidOperationException("The database connection has not been initialized.");
        internal set => _conn = value;
    }

    public bool IsConnectionAvailable => _conn is not null;

    public virtual DbTransaction? Transaction { get => _trans; internal set => _trans = value; }

    public virtual DbOptions Options { get; }

    public abstract string? DefaultDatabase { get; }

    public abstract ITypeMapper TypeMapper { get; }

    /// <summary>
    ///     Throws an exception if this object was already disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException" />
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public virtual void Dispose()
    {
        DisposeAsync().AsTask().Wait();
        GC.SuppressFinalize(this);
    }

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

    public async Task InvokeConnectionAsync(bool revoke = false, CancellationToken cancellationToken = default)
    {
        if (revoke && _conn is not null)
            await _conn.DisposeAsync();

        if (revoke || _conn is null)
            _conn = await OpenConnectionAsync(cancellationToken);
    }

    internal protected override ValueTask<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        return OpenConnectionAsync(cancellationToken);
    }

    public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public DbConnection CreateConnection()
    {
        return _dataSource.CreateConnection();
    }

    public abstract DbDataSource BuildDataSource(string connectionString);

    public abstract string ChangeDatabase(string database);

    public virtual void BeginTransaction()
    {
        BeginTransactionAsync().Wait();
    }

    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
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
            await _conn.OpenAsync(cancellationToken);

        _trans = await _conn.BeginTransactionAsync(cancellationToken);
    }

    public virtual void CommitTransaction()
    {
        CommitTransactionAsync().Wait();
    }

    public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_trans == null)
            throw new DbTransactionException("No transaction to be committed.");

        await _trans.CommitAsync(cancellationToken);
        _trans = null;
    }

    public virtual void RollbackTransaction()
    {
        RollbackTransactionAsync().Wait();
    }

    public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_trans == null)
            throw new DbTransactionException("No transaction to be rolled Wback.");

        await _trans.RollbackAsync(cancellationToken);
        _trans = null;
    }

    public override Command CreateCommand()
    {
        return new(this);
    }

    public override Command CreateCommand(DbConnection connection)
    {
        return new(this, connection);
    }

    public override Command CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null)
    {
        return new(this, sql, prms, timeout, type);
    }

    public virtual IQueryStore Query()
    {
        ThrowIfDisposed();

        var query = new DbQuery(this, _transactionExpected);
        _transactionExpected = false;

        return query;
    }

    public abstract Task TerminateDatabaseConnectionsAsync(string database, CancellationToken cancellationToken = default);
}