﻿using Sqlist.NET.Infrastructure;
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
        private readonly DbConnection _conn;
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
            _conn = _db.Connection;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbContextBase"/> implementation.</param>
        /// <param name="connection">A custom connection, which the command is to be executed over.</param>
        internal Command(DbContextBase db, DbConnection connection)
        {
            Check.NotNull(db, nameof(db));

            _db = db;
            _cmd = connection.CreateCommand();
            _conn = connection;
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
                if (value is BulkParameters prms)
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

        public void ConfigureBulkParameters(DbCommand cmd, BulkParameters prms)
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

        public void ConfigureParameters(DbCommand cmd, object? prms)
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
}
