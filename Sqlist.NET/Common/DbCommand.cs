using Sqlist.NET.Utilities;

using System;
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
            set
            {
                _prms = value;
                ConfigureParameters();
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

        private void EnsureConnectionOpen()
        {
            if (_conn.State == ConnectionState.Closed)
                _conn.Open();
        }

        private void ConfigureParameters()
        {
            if (_prms is null)
                return;

            foreach (var prop in _prms.GetType().GetProperties())
            {
                var prm = _cmd.CreateParameter();

                prm.ParameterName = prop.Name;
                prm.Direction = ParameterDirection.Input;
                prm.DbType = TypeMapper.Instance.ToDbType(prop.PropertyType);
                prm.Value = prop.GetValue(_prms) ?? DBNull.Value;

                _cmd.Parameters.Add(prm);
            }
        }
    }
}
