using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Migration;

/// <summary>
///     Initializes a new instance of the <see cref="PostgreDbManager"/> class.
/// </summary>
internal class MigrationService(
    IDbContext db,
    ISchemaBuilderFactory schemaFactory,
    ISqlBuilderFactory sqlFactory,
    IDataTransfer transfer,
    IOptions<MigrationOptions> options) : IMigrationService
{
    private readonly MigrationOptions _options = options.Value;
    private readonly string _schemaTable = options.Value.SchemaTable ?? Consts.DefaultSchemaTable;

    public async Task MigrateDataFromAsync(string dbname, DataTransactionMap dataMap, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

            await using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
            await transfer.CopyFromAsync(rdr, table, [.. definition.Columns], cancellationToken);
        }
    }

    public Task CreateDatabaseAsync(string database, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = schemaFactory.Create().CreateDatabase(database);
        var qry = db.Query();

        return qry.ExecuteAsync(sql, cancellationToken: cancellationToken);
    }

    public Task DeleteDatabaseAsync(string database, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = schemaFactory.Create().DeleteDatabase(database);
        var qry = db.Query();

        return qry.ExecuteAsync(sql, cancellationToken: cancellationToken);
    }

    public Task RenameDatabaseAsync(string currentName, string newName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = schemaFactory.Create().RenameDatabase(currentName, newName);
        var qry = db.Query();

        return qry.ExecuteAsync(sql, cancellationToken: cancellationToken);
    }

    public Task CreateSchemaTableAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var table = new SqlTable(_options.SchemaTableSchema, _schemaTable)
        {
            Columns =
            {
                new(Consts.Id, "int") { PrimaryKey = true },
                new(Consts.Version, "varchar (16)"),
                new(Consts.Package, "varchar (128)"),
                new(Consts.Parent, "int"),
                new(Consts.Title, "text"),
                new(Consts.Description, "text"),
                new(Consts.Summary, "text"),
                new(Consts.Applied, "timestamp without time zone", "current_timestamp")
            },
            Constraints =
            {
                ForeignKeys =
                {
                    new([Consts.Parent]) { References = new(_schemaTable, Consts.Id) }
                },
                Uniques =
                {
                    new([Consts.Package, Consts.Version])
                }
            }
        };

        var sql = schemaFactory.Create().CreateTable(table);
        var qry = db.Query();

        return qry.ExecuteAsync(sql, cancellationToken: cancellationToken);
    }

    public virtual Task<bool> DoesSchemaTableExistAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = sqlFactory.Sql("information_schema.tables");

        sql.RegisterFields("true");
        sql.Where($"table_name = '{_options.SchemaTable}'");

        if (!string.IsNullOrEmpty(_options.SchemaTableSchema))
            sql.AppendAnd($"table_schema = '{_options.SchemaTableSchema}'");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<bool>(stmt, cancellationToken: cancellationToken);
    }

    public virtual Task<SchemaPhase> GetLastSchemaPhaseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();
        sql.OrderBy(Consts.Version + " desc");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<SchemaPhase>(stmt, cancellationToken: cancellationToken);
    }

    public virtual Task<IEnumerable<SchemaPhase>> GetModularPhasesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();

        sql.Where(c => c.NotNull(Consts.Parent));
        sql.GroupBy(Consts.Package);
        sql.OrderBy(Consts.Applied + " desc");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.RetrieveAsync<SchemaPhase>(stmt, cancellationToken: cancellationToken);
    }

    public virtual Task<IEnumerable<SchemaPhase>> GetModularSchemaPhasesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();
        sql.OrderBy(Consts.Version);

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.RetrieveAsync<SchemaPhase>(stmt, cancellationToken: cancellationToken);
    }

    public virtual Task InsertSchemaPhaseAsync(SchemaPhase phase, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();

        sql.RegisterFields([
            Consts.Id,
            Consts.Version,
            Consts.Package,
            Consts.Parent,
            Consts.Title,
            Consts.Description,
            Consts.Summary,
            Consts.Applied
        ]);
        sql.RegisterValues(["@Id", "@Version", "@Package", "@Parent", "@Title", "@Description", "@Summary", "@Applied"]);

        var stmt = sql.ToInsert();
        var qry = db.Query();

        return qry.ExecuteAsync(stmt, new
        {
            phase.Id,
            phase.Version,
            phase.Package,
            phase.Parent,
            phase.Title,
            phase.Description,
            phase.Summary,
            phase.Applied
        }, cancellationToken: cancellationToken);
    }

    private SqlBuilder CreateSqlBuilder()
    {
        return new SqlBuilder(null, _options.SchemaTableSchema, _schemaTable);
    }
}
