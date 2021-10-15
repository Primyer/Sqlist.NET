using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using ado = System.Data.Common;

namespace Sqlist.NET.Common
{
    public class DbCommand
    {
        private readonly ado::DbConnection _conn;
        private readonly ado::DbCommand _cmd;

        private object _prms;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbCommand"/> class.
        /// </summary>
        /// <param name="conn">The ADO.NET connection.</param>
        internal DbCommand(ado::DbConnection conn)
        {
            Check.NotNull(conn, nameof(conn));

            _conn = conn;
            _cmd = conn.CreateCommand();
        }

        /// <summary>
        ///     Gets the underlying <see cref="ado::DbCommand"/>.
        /// </summary>
        public ado::DbCommand Underlying => _cmd;

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
        public object Parameters
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

        /// <inheritdoc cref="ado::DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        public virtual Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            return _cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <inheritdoc cref="ado::DbCommand.ExecuteScalarAsync(CancellationToken)"/>
        public virtual Task<object> ExecuteScalarAsync(CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            return _cmd.ExecuteScalarAsync(cancellationToken);
        }

        /// <summary>
        ///     Invokes a <see cref="ado::DbDataReader"/> returning a <see cref="LazyDbDataReader"/> that delays enumeration.
        /// </summary>
        /// <param name="commandBehavior">One of the <see cref="CommandBehavior"/> values.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation request.</param>
        /// <returns>The <see cref="LazyDbDataReader"/> object tahat delays enumertions.</returns>
        public virtual LazyDbDataReader ExecuteReaderAsync(CommandBehavior commandBehavior = default, CancellationToken cancellationToken = default)
        {
            EnsureConnectionOpen();
            var result = _cmd.ExecuteReaderAsync(commandBehavior, cancellationToken);

            return new LazyDbDataReader(result);
        }

        public static void ConfigureBulkParameters(ado::DbCommand cmd, object[][] prms)
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
                        prm.DbType = TypeMapper.Instance.ToDbType(val.GetType());
                        prm.Value = val;
                    }

                    cmd.Parameters.Add(prm);
                }
            }
        }

        public static void ConfigureParameters(ado::DbCommand cmd, object prms)
        {
            if (prms is null)
                return;

            IterateParamters(prms, (name, value) =>
            {
                var prm = cmd.CreateParameter();

                prm.ParameterName = name;
                prm.Direction = ParameterDirection.Input;

                if (value is null)
                    prm.Value = DBNull.Value;
                else
                {
                    prm.DbType = TypeMapper.Instance.ToDbType(value.GetType());
                    prm.Value = value;
                }

                cmd.Parameters.Add(prm);
            });
        }

        private static void IterateParamters(object prms, Action<string, object> predicate)
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
    }
}
