using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;

using NpgsqlTypes;

using Sqlist.NET.Data;
using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Infrastructure
{
    public class DbContext : DbContextBase
    {
        private readonly ILogger<DbContext>? _logger;
        private readonly NpgsqlConnectionStringBuilder _csBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration _options.</param>
        [ActivatorUtilitiesConstructor]
        public DbContext(IOptions<NpgsqlOptions> options, ILogger<DbContext>? logger = null) : this(options.Value, logger)
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContext"/> class.
        /// </summary>
        /// <param name="options">The Sqlist configuration _options.</param>
        public DbContext(NpgsqlOptions options, ILogger<DbContext>? logger = null) : base(options)
        {
            _logger = logger;
            _csBuilder = new NpgsqlConnectionStringBuilder(Options.ConnectionString);
        }

        /// <inheritdoc />
        public override NpgsqlConnection? Connection => base.Connection as NpgsqlConnection;

        /// <inheritdoc />
        public override NpgsqlTransaction? Transaction => base.Transaction as NpgsqlTransaction;

        /// <inheritdoc />
        public override string? DefaultDatabase => "postgres";

        public override NpgsqlOptions Options => (NpgsqlOptions)base.Options;

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
        public override async Task CopyAsync(DbConnection exporter, DbConnection importer, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Creating staging table for '{Table}'.", table);

            var stagingTable = table + "_staging";
            await CreateStagingTableAsync(stagingTable, rules);

            var row = 0;

            using (var reader = await ExecuteReaderAsync(exporter, table, rules, cancellationToken))
            using (var writer = await CreateImporterAsync(importer, stagingTable, rules, cancellationToken))
            {
                _logger?.LogInformation("Copying '{Table}' data...", table);

                while (await reader.ReadAsync(cancellationToken))
                {
                    await writer.StartRowAsync(cancellationToken);

                    foreach (var (name, rule) in rules)
                    {
                        if (rule.IsNew) continue;

                        var ordinal = reader.GetOrdinal(name);
                        var notEnum = !(rule.IsEnum ?? false);

                        var npgType = notEnum ? NpgsqlTypeMapper.GetNpgsqlDbType(rule.Type!) : NpgsqlDbType.Text;
                        var clrType = notEnum ? NpgsqlTypeMapper.Instance.GetType(rule.Type!) : typeof(string);

                        try
                        {
                            if (await reader.IsDBNullAsync(ordinal, cancellationToken))
                            {
                                if (npgType == NpgsqlDbType.Jsonb)
                                    await writer.WriteAsync<string>(null, "jsonb", cancellationToken);
                                else
                                    await writer.WriteNullAsync(cancellationToken);
                            }
                            else
                            {
                                var value = reader.GetValue(ordinal);
                                await writer.WriteAsync(value, npgType, cancellationToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Unexpected exception was thrown while copying column '{rule.ColumnName ?? name}' at row {row + 1}.", ex);
                        }
                    }
                    row++;
                }

                await writer.CompleteAsync(cancellationToken);
            }

            if (row != 0)
                await CommitDataAsync(stagingTable, table, rules);

            await DeleteStagingTableAsync(stagingTable);
            _logger?.LogInformation("Copy of '{Table}' data is completed.", table);
        }

        /// <inheritdoc />
        public override async Task CopyFromAsync(DbDataReader reader, string table, ICollection<KeyValuePair<string, string>> columns, CancellationToken cancellationToken = default)
        {
            var row = 1;
            var sql = new NpgsqlBuilder(table);
            sql.RegisterFields(columns.Select(c => c.Key).ToArray());

            var stmt = sql.ToCopyFrom(CopySource.StdIn, new CopyOptions { Format = "BINARY" });

            using (var writer = await Connection!.BeginBinaryImportAsync(stmt, cancellationToken))
            {
                _logger?.LogInformation("Transferring data to '{Table}'...", table);

                while (await reader.ReadAsync(cancellationToken))
                {
                    await writer.StartRowAsync(cancellationToken);

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var (name, type) = columns.ElementAt(i);

                        var dbType = NpgsqlTypeMapper.GetNpgsqlDbType(type);
                        var value = (dynamic)reader.GetValue(i);

                        try
                        {
                            if (value is null)
                                await writer.WriteNullAsync(cancellationToken); else
                                await writer.WriteAsync(value, dbType, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Unexpected exception was thrown while transferring to column '{name}' at row {row}.", ex);
                        }
                    }

                    row++;
                }

                await writer.CompleteAsync(cancellationToken);
            }

            _logger?.LogInformation("Transfer to table '{Table}' is completed.", table);
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
                        var type = (rule.IsEnum ?? false) ? "text" : rule.CurrentType!;

                        return new SqlColumn(rule.ColumnName ?? name, type);
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

        private Task<DbDataReader> ExecuteReaderAsync(DbConnection connection, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken)
        {
            var sql = Sql(table);

            var fields = rules.Where(entry => !entry.Value.IsNew)
                .Select(entry => (entry.Value.IsEnum ?? false) ? sql.Cast(entry.Key, "text") : entry.Key)
                .ToArray();

            sql.RegisterFields(fields);

            var cmd = CreateCommand(connection);
            cmd.Statement = sql.ToSelect();

            return cmd.ExecuteReaderAsync(cancellationToken: cancellationToken);
        }

        private static string CreateExportSqlStatement(string table, TransactionRuleDictionary rules)
        {
            return new NpgsqlBuilder().ToCopyTo(CopySource.StdOut, new CopyOptions { Format = "BINARY" }, sql =>
            {
                var trPairs = rules
                    .Where(entry => !entry.Value.IsNew)
                    .Select(entry =>
                    {
                        var (name, rule) = entry;
                        return (rule.IsEnum ?? false) ? sql.Cast(name, "text") : name;
                    })
                    .ToArray();

                sql.TableName = table;
                sql.RegisterFields(trPairs);

                return sql.ToSelect();
            });
        }

        private static Task<NpgsqlBinaryImporter> CreateImporterAsync(DbConnection connection, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken)
        {
            var sql = new NpgsqlBuilder(table);
            sql.RegisterFields(ToColumns(rules));

            var stmt = sql.ToCopyFrom(CopySource.StdIn, new CopyOptions { Format = "BINARY" });
            return ((NpgsqlConnection)connection).BeginBinaryImportAsync(stmt, cancellationToken);
        }

        private async Task CommitDataAsync(string stagingTable, string table, TransactionRuleDictionary rules)
        {
            var sql = Sql(table);

            var destColumns = rules
                .Where(entry => !entry.Value.IsNew || !string.IsNullOrEmpty(entry.Value.Value))
                .Select(entry => entry.Value.ColumnName ?? entry.Key)
                .ToArray();

            sql.RegisterFields(destColumns);

            var stmt = sql.ToInsert(sub =>
            {
                sub.TableName = stagingTable;
                
                foreach (var (name, rule) in rules)
                {
                    if (rule.IsNew && string.IsNullOrEmpty(rule.Value))
                        continue;

                    var colName = rule.ColumnName ?? name;
                    var column = (rule.IsEnum ?? false) ? sql.Cast(colName, rule.Type!) : colName;

                    if (string.IsNullOrEmpty(rule.Value))
                        sub.RegisterFields(column);
                    else
                    {
                        var value = rule.Value.Replace("{column}", column);
                        sub.RegisterFields(value, false);
                    }
                }
            });

            var sequences = rules.Where(rule => rule.Value.IsSequence);
            if (sequences.Any())
            {
                sql = Sql(table);

                foreach (var (name, rule) in sequences)
                {
                    var colName = rule.ColumnName ?? name;

                    if (rule.Inherits is not null)
                        sql.TableName = rule.Inherits;

                    var sequenceName = string.IsNullOrWhiteSpace(rule.SequenceName)
                        ? $"pg_get_serial_sequence('{sql.TableName}', '{colName}')"
                        : $"'{rule.SequenceName}'";

                    sql.RegisterFields($"setval({sequenceName}, max(`{colName}`))");
                }
            }

            stmt += ";\n" + sql.ToSelect();
            await Query().ExecuteAsync(stmt);
        }

        private static string[] ToColumns(TransactionRuleDictionary rules)
        {
            return rules
                .Where(rule => !rule.Value.IsNew)
                .Select(rule => rule.Value.ColumnName ?? rule.Key)
                .ToArray();
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
        public override async Task TerminateDatabaseConnectionsAsync(string database)
        {
            if (database == Connection?.Database)
                await Connection.ChangeDatabaseAsync(DefaultDatabase!);

            var sql = $"""
                REVOKE CONNECT ON DATABASE {database} FROM PUBLIC, {_csBuilder.Username};

                SELECT pg_terminate_backend(pid)
                FROM pg_stat_activity
                WHERE pid <> pg_backend_pid() AND datname = '{database}';
                """;

            await Query().ExecuteAsync(sql);

            await using var connection = new NpgsqlConnection(ChangeDatabase(database));
            NpgsqlConnection.ClearPool(connection);
        }
    }
}
