using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using Sqlist.NET.Data;
using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Infrastructure
{
    public class DbContext : DbContextBase<NpgsqlConnectionStringBuilder>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        [ActivatorUtilitiesConstructor]
        public DbContext(IOptions<DbOptions> options) : this(options.Value)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbContext(DbOptions options) : base(NpgsqlFactory.Instance, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration options.</param>
        public DbContext(DbOptions options, Action<NpgsqlConnectionStringBuilder> connectionString) : base(NpgsqlFactory.Instance, options, connectionString)
        {
        }

        public override string? DefaultDatabase => "postgres";

        /// <inheritdoc />
        protected override string ChangeDatabase(string database)
        {
            var current = ConnectionStringBuilder.Database;

            ConnectionStringBuilder.Database = database;
            var connStr = ConnectionString;

            ConnectionStringBuilder.Database = current;
            return connStr;
        }

        /// <inheritdoc />
        public override async Task CopyAsync(DbConnection source, DbConnection destination, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
        {
            var options = new CopyOptions { Format = "BINARY" };
            var trPairs = rules.Where(rule => !rule.Value.IsNew);

            var sql = new NpgsqlBuilder(table);
            sql.RegisterFields(trPairs.Select(pair => pair.Key).ToArray());

            var copyToStmt = sql.ToCopyTo(CopySource.StdOut, options);

            sql.ClearFields();
            sql.RegisterFields(trPairs.Select(rule => rule.Value.ColumnName ?? rule.Key).ToArray());

            var copyFromStmt = sql.ToCopyFrom(CopySource.StdIn, options);

            using var reader = await ((NpgsqlConnection)source).BeginBinaryExportAsync(copyToStmt, cancellationToken);
            using var writer = await ((NpgsqlConnection)destination).BeginBinaryImportAsync(copyFromStmt, cancellationToken);

            while (await reader.StartRowAsync(cancellationToken) != -1)
            {
                await writer.StartRowAsync(cancellationToken);

                for (var i = 0; i < rules.Keys.Count; i++)
                {
                    if (reader.IsNull)
                        await writer.WriteNullAsync(cancellationToken);
                    else
                    {
                        var value = await reader.ReadAsync<object>(cancellationToken);
                        await writer.WriteAsync(value, cancellationToken);
                    }
                }
            }

            await writer.CompleteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override SqlBuilder Sql(Encloser? encloser, string? schema, string? table)
        {
            return table is null
                ? new NpgsqlBuilder(encloser)
                : new NpgsqlBuilder(encloser, schema, table);
        }

        /// <inheritdoc />
        public override void TerminateDatabaseConnections(string database)
        {
//            var sql = @$"
//REVOKE CONNECT ON DATABASE {database} FROM PUBLIC, {ConnectionStringBuilder.Username};

//SELECT pg_terminate_backend(pid)
//FROM pg_stat_activity
//WHERE pid <> pg_backend_pid() AND datname = '{database}';";

//            await Query().ExecuteAsync(sql);

            var conn = new NpgsqlConnection(ChangeDatabase(database));
            NpgsqlConnection.ClearPool(conn);
        }
    }
}
