using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;
using Sqlist.NET.Sql;
using Sqlist.NET.Utilities;

namespace Sqlist.NET.Migration;

internal class MigrationService(IDbContext db, ISchemaBuilderFactory schemaFactory, ISqlBuilderFactory sqlFactory,
    IDataTransfer transfer, IOptions<MigrationOptions> options) : IMigrationService
{
    private readonly MigrationOptions _options = options.Value;
    
    private readonly SqlTable _schemaTableDefinition = new SqlTable(
        options.Value.SchemaTableSchema, options.Value.SchemaTable)
    {
        Columns =
        {
            new(Consts.Id, db.TypeMapper.TypeName<int>(), true)
            {
                PrimaryKey = true,
                AutoIncrement = true
            },
            new(Consts.Version, db.TypeMapper.TypeName<string>(16)),
            new(Consts.Package, db.TypeMapper.TypeName<string>(128)),
            new(Consts.Parent, db.TypeMapper.TypeName<int>()),
            new(Consts.Title, db.TypeMapper.TypeName<string>()),
            new(Consts.Description, db.TypeMapper.TypeName<string>()),
            new(Consts.Summary, db.TypeMapper.TypeName<string>()),
            new(Consts.Applied, db.TypeMapper.TypeName<DateTime>(), "CURRENT_TIMESTAMP")
        },
        Constraints =
        {
            ForeignKeys =
            {
                new([Consts.Parent]) { References = new(options.Value.SchemaTable, Consts.Id) }
            },
            Uniques =
            {
                new([Consts.Package, Consts.Version])
            }
        }
    };

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

        var sql = schemaFactory.Create().CreateTable(_schemaTableDefinition);
        var qry = db.Query();

        return qry.ExecuteAsync(sql, cancellationToken: cancellationToken);
    }

    public Task<bool> DoesSchemaTableExistAsync(CancellationToken cancellationToken)
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

    public Task<SchemaPhase> GetLastSchemaPhaseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();
        sql.OrderBy(Consts.Version + " desc");

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<SchemaPhase>(stmt, cancellationToken: cancellationToken);
    }
    
    public Task<IEnumerable<SchemaPhase>> GetModularSchemaPhasesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();
        sql.OrderBy(Consts.Version);

        var stmt = sql.ToSelect();
        var qry = db.Query();

        return qry.RetrieveAsync<SchemaPhase>(stmt, cancellationToken: cancellationToken);
    }

    public Task<int> InsertSchemaPhaseAsync(SchemaPhase phase, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();

        sql.RegisterFields([
            Consts.Id, Consts.Version, Consts.Package, Consts.Parent, Consts.Title, Consts.Description, Consts.Summary,
            Consts.Applied
        ]);
        sql.RegisterValues([
            "@Id", "@Version", "@Package", "@Parent", "@Title", "@Description", "@Summary", "@Applied"
        ]);
        sql.RegisterReturningFields(Consts.Id);

        var stmt = sql.ToInsert();
        var qry = db.Query();

        return qry.FirstOrDefaultAsync<int>(stmt, new
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

    public Task InsertSchemaPhasesAsync(IEnumerable<SchemaPhase> phases, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sql = CreateSqlBuilder();
        var fields = new[]
        {
            Consts.Id, Consts.Version, Consts.Package, Consts.Parent, Consts.Title, Consts.Description,
            Consts.Summary, Consts.Applied
        };

        sql.RegisterFields(fields);
        sql.RegisterBulkValues(fields.Length, phases.Count());

        var stmt = sql.ToInsert();
        var prms = new BulkParameters(phases.Select(phase => new
        {
            phase.Id,
            phase.Version,
            phase.Package,
            phase.Parent,
            phase.Title,
            phase.Description,
            phase.Summary,
            phase.Applied
        }));

        return db.Query().FirstOrDefaultAsync<int>(stmt, prms, cancellationToken: cancellationToken);
    }

    private SqlBuilder CreateSqlBuilder()
    {
        return new SqlBuilder(null, _options.SchemaTableSchema, _options.SchemaTable);
    }
}
