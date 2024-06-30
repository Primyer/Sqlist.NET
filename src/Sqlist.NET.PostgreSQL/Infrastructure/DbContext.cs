using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Infrastructure
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContext"/> class.
    /// </summary>
    /// <param name="options">The Sqlist configuration options.</param>
    public class DbContext(IOptions<NpgsqlOptions> options) : DbContextBase(options.Value)
    {
        private readonly NpgsqlConnectionStringBuilder _csBuilder = new (options.Value.ConnectionString);

        /// <inheritdoc />
        public override NpgsqlConnection? Connection => base.Connection as NpgsqlConnection;

        /// <inheritdoc />
        public override NpgsqlTransaction? Transaction => base.Transaction as NpgsqlTransaction;

        /// <inheritdoc />
        public override string? DefaultDatabase => "postgres";

        public override NpgsqlOptions Options => options.Value;

        /// <inheritdoc />
        public override TypeMapper TypeMapper => NpgsqlTypeMapper.Instance;

        public override NpgsqlDataSource BuildDataSource(string? connectionString = null)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString ?? Options.ConnectionString);
            Options.ConfigureDataSource?.Invoke(dataSourceBuilder);

            return dataSourceBuilder.Build();
        }

        /// <inheritdoc />
        public override string ChangeDatabase(string database)
        {
            var csBuilder = new NpgsqlConnectionStringBuilder(Options.ConnectionString) { Database = database };
            return csBuilder.ConnectionString;
        }

        /// <inheritdoc />
        public override async Task TerminateDatabaseConnectionsAsync(string database, CancellationToken cancellationToken = default)
        {
            if (database == Connection?.Database)
                await Connection.ChangeDatabaseAsync(DefaultDatabase!, cancellationToken);

            var sql = $"""
                REVOKE CONNECT ON DATABASE {database} FROM PUBLIC, {_csBuilder.Username};

                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE pid <> pg_backend_pid() AND datname = '{database}';
                """;

            await Query().ExecuteAsync(sql, cancellationToken: cancellationToken);

            await using var connection = new NpgsqlConnection(ChangeDatabase(database));
            NpgsqlConnection.ClearPool(connection);
        }
    }
}
