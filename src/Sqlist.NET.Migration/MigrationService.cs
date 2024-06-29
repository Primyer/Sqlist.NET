using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Data;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Sql;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration;

/// <summary>
///     Initializes a new instance of the <see cref="PostgreDbManager"/> class.
/// </summary>
public class MigrationService(IDbContext db, ISchemaBuilderFactory schemaFactory, ISqlBuilderFactory sqlFactory, IDataTransfer transfer, MigrationOptions options)
{
    public async Task MigrateDataFromAsync(string dbname, DataTransactionMap dataMap)
    {
        var cancellationToken = default(CancellationToken);
        await using var dataSource = db.BuildDataSource(db.ChangeDatabase(dbname));

        foreach (var (table, rules) in dataMap)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (rules.All(rule => rule.Value.IsNew))
                continue;

            await using var cnn = await dataSource.OpenConnectionAsync(cancellationToken);
            await transfer.CopyFromAsync(cnn, table, rules, cancellationToken);
        }

        foreach (var (table, definition) in dataMap.TransferDefinitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var cnn = await dataSource.OpenConnectionAsync(cancellationToken);
            await using var cmd = cnn.CreateCommand();

            cmd.CommandText = definition.Script;

            await using var rdr = await cmd.ExecuteReaderAsync();
            await transfer.CopyFromAsync(rdr, table, [.. definition.Columns], cancellationToken);
        }
    }

    public Task CreateDatabaseAsync(string database)
    {
        var sql = schemaFactory.Create().CreateDatabase(database);
        var qry = db.Query();

        return qry.ExecuteAsync(sql);
    }

    public Task DeleteDatabaseAsync(string database)
    {
        var sql = schemaFactory.Create().DeleteDatabase(database);
        var qry = db.Query();

        return qry.ExecuteAsync(sql);
    }

    public Task RenameDatabaseAsync(string currentName, string newName)
    {
        var sql = schemaFactory.Create().RenameDatabase(currentName, newName);
        var qry = db.Query();

        return qry.ExecuteAsync(sql);
    }

    public Task CreateSchemaTableAsync()
    {
        var table = new SqlTable(options.SchemaTableSchema, options.SchemaTable!)
        {
            Columns =
            {
                new SqlColumn("version", "varchar (16)") { PrimaryKey = true },
                new SqlColumn("title", "text"),
                new SqlColumn("description", "text"),
                new SqlColumn("summary", "text"),
                new SqlColumn("applied", "timestamp without time zone", "current_timestamp")
            }
        };

        var sql = schemaFactory.Create().CreateTable(table);
        var qry = db.Query();

        return qry.ExecuteAsync(sql);
    }

    public virtual Task<bool> DoesSchemaTableExistAsync()
    {
        var sql = sqlFactory.Sql("information_schema.tables");

        sql.RegisterFields("true");
        sql.Where($"table_name = '{options.SchemaTable}'");

        if (!string.IsNullOrEmpty(options.SchemaTableSchema))
            sql.AppendAnd($"table_schema = '{options.SchemaTableSchema}'");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<bool>(stmt);
    }

    public virtual Task<SchemaPhase> GetLastSchemaPhaseAsync()
    {
        var sql = new SqlBuilder(null, options.SchemaTableSchema, options.SchemaTable!);
        sql.OrderBy("version desc");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<SchemaPhase>(stmt);
    }

    public virtual Task<IEnumerable<SchemaPhase>> GetSchemaPhasesAsync()
    {
        var sql = new SqlBuilder(null, options.SchemaTableSchema, options.SchemaTable!);
        sql.OrderBy("version");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.RetrieveAsync<SchemaPhase>(stmt);
    }

    public virtual Task InsertSchemaPhaseAsync(SchemaPhase phase)
    {
        var sql = new SqlBuilder(null, options.SchemaTableSchema, options.SchemaTable!);

        sql.RegisterFields(["version", "title", "description", "summary", "applied"]);
        sql.RegisterValues(["@Version", "@Title", "@Description", "@Summary", "@Applied"]);

        var stmt = sql.ToInsert();
        var qry = db.Query();

        return qry.ExecuteAsync(stmt, new
        {
            phase.Version,
            phase.Title,
            phase.Description,
            phase.Summary,
            phase.Applied
        });
    }
}
