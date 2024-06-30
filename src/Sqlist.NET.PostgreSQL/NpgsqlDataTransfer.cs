using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using Sqlist.NET.Data;
using Sqlist.NET.Metadata;
using Sqlist.NET.Sql;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using System;
using Sqlist.NET.Infrastructure;
using Npgsql;
using System.Linq;

namespace Sqlist.NET;
public class NpgsqlDataTransfer(DbContext db, ISchemaBuilderFactory schemaFactory, ILogger<NpgsqlDataTransfer> logger) : IDataTransfer
{
    public DbConnection? Connection => db.Connection;

    /// <inheritdoc />
    public async Task CopyAsync(DbConnection exporter, DbConnection importer, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
    {
        logger?.LogInformation("Creating staging table for '{Table}'.", table);

        var stagingTable = table + "_staging";
        await CreateStagingTableAsync(stagingTable, rules);
        var row = 0;

        using (var reader = await ExecuteReaderAsync(exporter, table, rules, cancellationToken))
        using (var writer = await CreateImporterAsync(importer, stagingTable, rules, cancellationToken))
        {
            logger?.LogInformation("Copying '{Table}' data...", table);

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
                                await writer.WriteAsync<string?>(null, "jsonb", cancellationToken);
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
        logger?.LogInformation("Copy of '{Table}' data is completed.", table);
    }

    /// <inheritdoc />
    public async Task CopyFromAsync(DbDataReader reader, string table, ICollection<KeyValuePair<string, string>> columns, CancellationToken cancellationToken = default)
    {
        var row = 1;
        var sql = new NpgsqlBuilder(table);
        sql.RegisterFields(columns.Select(c => c.Key).ToArray());

        var stmt = sql.ToCopyFrom(CopySource.StdIn, new CopyOptions { Format = "BINARY" });

        using (var writer = await db.Connection!.BeginBinaryImportAsync(stmt, cancellationToken))
        {
            logger?.LogInformation("Transferring data to '{Table}'...", table);

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
                            await writer.WriteNullAsync(cancellationToken);
                        else
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

        logger?.LogInformation("Transfer to table '{Table}' is completed.", table);
    }

    private Task<int> CreateStagingTableAsync(string table, TransactionRuleDictionary rules)
    {
        var sql = schemaFactory.Create().CreateTable(new SqlTable(table)
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

        return db.Query().ExecuteAsync(sql);
    }

    private Task<int> DeleteStagingTableAsync(string table)
    {
        var sql = schemaFactory.Create().DeleteTable(table);
        return db.Query().ExecuteAsync(sql);
    }

    private Task<DbDataReader> ExecuteReaderAsync(DbConnection connection, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken)
    {
        var sql = new NpgsqlBuilder(table);

        var fields = rules.Where(entry => !entry.Value.IsNew)
            .Select(entry => (entry.Value.IsEnum ?? false) ? sql.Cast(entry.Key, "text") : entry.Key)
            .ToArray();

        sql.RegisterFields(fields);

        var cmd = db.CreateCommand(connection);
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
        var sql = new NpgsqlBuilder(table);

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
            sql = new NpgsqlBuilder(table);

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
        await db.Query().ExecuteAsync(stmt);
    }

    private static string[] ToColumns(TransactionRuleDictionary rules)
    {
        return rules
            .Where(rule => !rule.Value.IsNew)
            .Select(rule => rule.Value.ColumnName ?? rule.Key)
            .ToArray();
    }
}
