using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Npgsql;

using NpgsqlTypes;

using Sqlist.NET.Data;
using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace Sqlist.NET.Infrastructure
{
    public class DbContext : DbContextBase<NpgsqlConnectionStringBuilder>
    {
        private static readonly MethodInfo ExporterReadMethod = typeof(NpgsqlBinaryExporter)
            .GetMethod("ReadAsync", 1, new[]
            {
                typeof(NpgsqlDbType),
                typeof(CancellationToken)
            });

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

        /// <inheritdoc />
        public override string? DefaultDatabase => "postgres";

        /// <inheritdoc />
        public override TypeMapper TypeMapper => NpgsqlTypeMapper.Instance;

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
        public override async Task CopyAsync(DbConnection exporter, DbConnection importer, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
        {
            var stagingTable = table + "_staging";
            await CreateStagingTableAsync(stagingTable, rules);

            var exportStmt = CreateExportSqlStatement(table, rules);
            var importStmt = CreateImportSqlStatement(stagingTable, rules);
            var rowIndex = 1;

            using (var reader = await ((NpgsqlConnection)exporter).BeginBinaryExportAsync(exportStmt, cancellationToken))
            using (var writer = await ((NpgsqlConnection)importer).BeginBinaryImportAsync(importStmt, cancellationToken))
            {
                while (await reader.StartRowAsync(cancellationToken) != -1)
                {
                    await writer.StartRowAsync(cancellationToken);

                    foreach (var (name, rule) in rules)
                    {
                        if (rule.IsNew) continue;
                        if (reader.IsNull)
                        {
                            await reader.SkipAsync(cancellationToken);
                            await writer.WriteNullAsync(cancellationToken);
                        }
                        else
                        {
                            try
                            {
                                var value = await ReadAsync(reader, rule, cancellationToken);
                                await writer.WriteAsync(value, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Unexpected exception was thrown while copying column '{rule.ColumnName ?? name}' at row {rowIndex}.", ex);
                            }
                        }
                    }

                    rowIndex++;
                }

                await writer.CompleteAsync(cancellationToken);
            }

            await CommitDataAsync(stagingTable, table, rules);
            await DeleteStagingTableAsync(stagingTable);
        }

        private Task CreateStagingTableAsync(string table, TransactionRuleDictionary rules)
        {
            var sql = Sql().CreateTable(new SqlTable(table)
            {
                Columns = rules
                    .Where(rule => !rule.Value.IsNew)
                    .Select(entry =>
                    {
                        var (name, rule) = entry;
                        return new SqlColumn(rule.ColumnName ?? name, rule.CurrentType!);
                    })
                    .ToArray()
            });

            return Query().ExecuteAsync(sql);
        }

        private Task DeleteStagingTableAsync(string table)
        {
            var sql = Sql().DeleteTable(table);
            return Query().ExecuteAsync(sql);
        }

        private static string CreateExportSqlStatement(string table, TransactionRuleDictionary rules)
        {
            var trPairs = rules
                .Where(rule => !rule.Value.IsNew)
                .Select(pair => pair.Key)
                .ToArray();

            var sql = new NpgsqlBuilder(table);
            sql.RegisterFields(trPairs);

            return sql.ToCopyTo(CopySource.StdOut, new CopyOptions { Format = "BINARY" });
        }

        private static string CreateImportSqlStatement(string table, TransactionRuleDictionary rules)
        {
            var sql = new NpgsqlBuilder(table);
            sql.RegisterFields(ToColumns(rules));

            return sql.ToCopyFrom(CopySource.StdIn, new CopyOptions { Format = "BINARY" });
        }

        private Task CommitDataAsync(string stagingTable, string table, TransactionRuleDictionary rules)
        {
            var destColumns = rules
                .Where(entry => !entry.Value.IsNew || !string.IsNullOrEmpty(entry.Value.Value))
                .Select(entry =>
                {
                    var (name, rule) = entry;
                    return rule.ColumnName ?? name;
                });

            var sql = Sql(table);
            sql.RegisterFields(destColumns.ToArray());

            var stmt = sql.ToInsert(sub =>
            {
                sub.TableName = stagingTable;

                foreach (var (name, rule) in rules)
                {
                    if (rule.IsNew && string.IsNullOrEmpty(rule.Value))
                        continue;

                    var column = rule.ColumnName ?? name;

                    if (string.IsNullOrEmpty(rule.Value))
                        sub.RegisterFields(column);
                    else
                    {
                        var value = rule.Value.Replace("{column}", column);
                        sub.RegisterFields(value, false);
                    }
                }
            });

            return Query().ExecuteAsync(stmt);
        }

        private static string[] ToColumns(TransactionRuleDictionary rules)
        {
            return rules
                .Where(rule => !rule.Value.IsNew)
                .Select(rule => rule.Value.ColumnName ?? rule.Key)
                .ToArray();
        }

        private static async Task<object?> ReadAsync(NpgsqlBinaryExporter reader, DataTransactionRule rule, CancellationToken cancellationToken)
        {
            var clrType = NpgsqlTypeMapper.Instance.GetType(rule.Type!);
            var npgType = NpgsqlTypeMapper.Instance.GetNpgsqlDbType(rule.Type!);

            return await (dynamic)ExporterReadMethod
                .MakeGenericMethod(clrType)
                .Invoke(reader, new object[] { npgType, cancellationToken });
        }

        /// <inheritdoc />
        public override SqlBuilder Sql(Encloser? encloser, string? schema, string? table)
        {
            encloser ??= new NpgsqlEncloser();

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
