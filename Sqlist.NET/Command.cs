using Sqlist.NET.Infrastructure;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET
{
    public class Command : IDisposable, IAsyncDisposable
    {
        private readonly DbContextBase _db;
        private readonly DbCommand _cmd;

        private object? _prms;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbContextBase"/> implementation.</param>
        internal Command(DbContextBase db)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
            _cmd = _db.Connection!.CreateCommand();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbContextBase"/> implementation.</param>
        /// <param name="sql">The SQL statement to run against the data source.</param>
        /// <param name="prms">The parameters associated with the given statement.</param>
        /// <param name="timeout">The wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="type">The type that indicates how SQL statement is interpreted.</param>
        internal Command(DbContextBase db, string sql, object? prms = null, int? timeout = null, CommandType? type = null) : this(db)
        {
            Statement = sql;
            Parameters = prms;
            Timeout = timeout;
            Type = type;
        }

        /// <summary>
        ///     Gets the underlying <see cref="DbCommand"/>.
        /// </summary>
        public DbCommand Underlying => _cmd;

        /// <summary>
        ///     Gets or sets the SQL statement to run against the data source.
        /// </summary>
        public string Statement
        {
            get => _cmd.CommandText;
            set
            {
                _cmd.CommandText = value;
            }
        }

        /// <summary>
        ///     Gets or sets the parameters associated with the given configured statement.
        /// </summary>
        public object? Parameters
        {
            get => _prms;
            internal set
            {
                if (value is object[][] prms)
                    ConfigureBulkParameters(_cmd, prms);
                else
                    ConfigureParameters(_cmd, value);

                _prms = value;
            }
        }

        /// <summary>
        ///     Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        public int? Timeout
        {
            get => _cmd.CommandTimeout;
            set
            {
                if (value.HasValue)
                    _cmd.CommandTimeout = value.Value;
            }
        }

        /// <summary>
        ///     Gets or sets the type that indicates how SQL statement is interpreted.
        /// </summary>
        public CommandType? Type
        {
            get => _cmd.CommandType;
            set
            {
                _cmd.CommandType = value ?? CommandType.Text;
            }
        }

        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        public virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            return _cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc cref="DbCommand.ExecuteScalarAsync(CancellationToken)"/>
        public virtual Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            return _cmd.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        ///     Prepares <see cref="DbDataReader"/> by returning a <see cref="LazyDbDataReader"/> that delays enumeration.
        /// </summary>
        /// <param name="commandBehavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
        /// <returns>The <see cref="LazyDbDataReader"/> object tahat delays enumertions.</returns>
        public virtual LazyDbDataReader PrepareReader(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default)
        {
            var readerTask = ExecuteReaderAsync(commandBehavior, cancellationToken);
            return new LazyDbDataReader(readerTask);
        }

        /// <summary>Invokes <see cref="DbDataReader"/>.</summary>
        /// <param name="commandBehavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
        /// <returns>
        ///     The <see cref="Task"/> that represent the asynchronous operation, containing the invoked <see cref="DbDataReader"/>.
        /// </returns>
        public virtual Task<DbDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            return _cmd.ExecuteReaderAsync(commandBehavior, cancellationToken);
        }

        public void ConfigureBulkParameters(DbCommand cmd, object[][] prms)
        {
            if (prms is null || prms.Length == 0)
                return;

            var rowCount = prms.Length;
            var colCount = prms[0].Length;

            for (var i = 0; i < rowCount; i++)
            {
                for (var j = 0; j < colCount; j++)
                {
                    var prm = cmd.CreateParameter();
                    var val = prms[i][j];

                    prm.ParameterName = "p" + (j + i * colCount);
                    prm.Direction = ParameterDirection.Input;

                    if (val is null)
                        prm.Value = DBNull.Value;
                    else
                    {
                        prm.DbType = _db.TypeMapper.ToDbType(val.GetType());
                        prm.Value = val;
                    }

                    cmd.Parameters.Add(prm);
                }
            }
        }

        public void ConfigureParameters(DbCommand cmd, object? prms)
        {
            if (prms is null)
                return;

            IterateParamters(prms, (name, value) =>
            {
                if (value is object[][] bulk)
                {
                    ConfigureBulkParameters(cmd, bulk);
                    return;
                }

                var prm = cmd.CreateParameter();

                prm.ParameterName = name;
                prm.Direction = ParameterDirection.Input;

                if (value is null)
                    prm.Value = DBNull.Value;
                else
                {
                    prm.DbType = _db.TypeMapper.ToDbType(value.GetType());
                    prm.Value = value;
                }

                cmd.Parameters.Add(prm);
            });
        }

        private static void IterateParamters(object prms, Action<string, object?> predicate)
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
            if (_db.Connection!.State == ConnectionState.Closed)
                _db.Connection.Open();
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
}
