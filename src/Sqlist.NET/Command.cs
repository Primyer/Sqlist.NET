using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System.Data;
using System.Data.Common;

namespace Sqlist.NET;
public class Command : ICommand
{
    private readonly DbContextBase _db;
    private readonly DbCommand _cmd;
    private readonly DbConnection _conn;
    private object? _prms;

    internal Command(DbContextBase db)
    {
        Check.NotNull(db, nameof(db));

        _db = db;
        _cmd = _db.Connection.CreateCommand();
        _conn = _db.Connection;
    }

    internal Command(DbContextBase db, DbConnection connection)
    {
        Check.NotNull(db, nameof(db));

        _db = db;
        _cmd = connection.CreateCommand();
        _conn = connection;
    }

    internal Command(DbContextBase db, string sql, object? prms = null, int? timeout = null, CommandType? type = null) : this(db)
    {
        Statement = sql;
        Parameters = prms;
        Timeout = timeout;
        Type = type;
    }

    public DbCommand Underlying => _cmd;

    public string Statement
    {
        get => _cmd.CommandText;
        set
        {
            _cmd.CommandText = value;
        }
    }

    public object? Parameters
    {
        get => _prms;
        set
        {
            if (value is BulkParameters prms)
                ConfigureBulkParameters(_cmd, prms);
            else
                ConfigureParameters(_cmd, value);

            _prms = value;
        }
    }

    public int? Timeout
    {
        get => _cmd.CommandTimeout;
        set
        {
            if (value.HasValue)
                _cmd.CommandTimeout = value.Value;
        }
    }

    public CommandType? Type
    {
        get => _cmd.CommandType;
        set
        {
            _cmd.CommandType = value ?? CommandType.Text;
        }
    }

    public virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnectionOpen();
        return _cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public virtual Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnectionOpen();
        return _cmd.ExecuteScalarAsync(cancellationToken);
    }

    public virtual ILazyDataReader PrepareReader(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default)
    {
        var readerTask = ExecuteReaderAsync(commandBehavior, cancellationToken);
        return new LazyDbDataReader(readerTask);
    }

    public virtual Task<DbDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default)
    {
        EnsureConnectionOpen();
        return _cmd.ExecuteReaderAsync(commandBehavior, cancellationToken);
    }

    internal void ConfigureBulkParameters(DbCommand cmd, BulkParameters prms)
    {
        var (i, j) = (0, 0);

        foreach (var obj in prms)
        {
            foreach (var (value, type) in obj)
            {
                var param = cmd.CreateParameter();
                var nType = Nullable.GetUnderlyingType(type) ?? type;

                param.ParameterName = "p" + (j++ + i * obj.Length);
                param.Direction = ParameterDirection.Input;
                param.DbType = _db.TypeMapper.ToDbType(nType);
                param.Value = value switch
                {
                    null => DBNull.Value,
                    Enumeration @enum => @enum.DisplayName,
                    _ => value
                };

                cmd.Parameters.Add(param);
            }

            j = 0;
            i++;
        }
    }

    internal void ConfigureParameters(DbCommand cmd, object? prms)
    {
        if (prms is null)
            return;

        IterateParamters(prms, (name, value) =>
        {
            if (value is BulkParameters bulk)
            {
                ConfigureBulkParameters(cmd, bulk);
                return;
            }

            var prm = cmd.CreateParameter();

            prm.ParameterName = name;
            prm.Direction = ParameterDirection.Input;
            prm.Value = value switch
            {
                null => DBNull.Value,
                Enumeration @enum => @enum.DisplayName,
                _ => value
            };

            cmd.Parameters.Add(prm);
        });
    }

    internal static void IterateParamters(object prms, Action<string, object?> predicate)
    {
        if (prms is IDictionary<string, object> dict)
        {
            foreach (var (key, value) in dict)
                predicate(key, value);

            return;
        }

        foreach (var prop in prms.GetType().GetProperties())
            predicate(prop.Name, prop.GetValue(prms));
    }

    private void EnsureConnectionOpen()
    {
        if (_conn.State == ConnectionState.Closed)
            _conn.Open();
    }

    public async ValueTask DisposeAsync()
    {
        await _cmd.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        _cmd.Dispose();
        GC.SuppressFinalize(this);
    }
}