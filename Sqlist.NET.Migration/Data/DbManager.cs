using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Sql;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration.Data
{
    public class DbManager
    {
        private readonly DbContextBase _db;
        private readonly MigrationOptions _options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreDbManager"/> class.
        /// </summary>
        public DbManager(DbContextBase db, MigrationOptions options)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task MigrateDataFromAsync(string dbname, DataTransactionMap dataMap)
        {
            var cancellationToken = default(CancellationToken);
            await using var dataSource = _db.BuildDataSource(_db.ChangeDatabase(dbname));

            foreach (var (table, rules) in dataMap)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (rules.All(rule => rule.Value.IsNew))
                    continue;

                await using var cnn = await dataSource.OpenConnectionAsync(cancellationToken);
                await _db.CopyFromAsync(cnn, table, rules, cancellationToken);
            }

            foreach (var (table, definition) in dataMap.TransferDefinitions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var cnn = await dataSource.OpenConnectionAsync(cancellationToken);
                await using var cmd = cnn.CreateCommand();

                cmd.CommandText = definition.Script;

                await using var rdr = await cmd.ExecuteReaderAsync();
                await _db.CopyFromAsync(rdr, table, definition.Columns, cancellationToken);
            }
        }

        public Task CreateDatabaseAsync(string database)
        {
            var sql = _db.UncasedSql().CreateDatabase(database);
            var qry = _db.Query();

            return qry.ExecuteAsync(sql);
        }

        public Task DeleteDatabaseAsync(string database)
        {
            var sql = _db.UncasedSql().DeleteDatabase(database);
            var qry = _db.Query();

            return qry.ExecuteAsync(sql);
        }

        public Task RenameDatabaseAsync(string currentName, string newName)
        {
            var sql = _db.UncasedSql().RenameDatabase(currentName, newName);
            var qry = _db.Query();

            return qry.ExecuteAsync(sql);
        }

        public Task CreateSchemaTableAsync()
        {
            var table = new SqlTable(_options.SchemaTableSchema, _options.SchemaTable!)
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

            var sql = _db.UncasedSql().CreateTable(table);
            var qry = _db.Query();

            return qry.ExecuteAsync(sql);
        }

        public virtual Task<bool> DoesSchemaTableExistAsync()
        {
            var sql = _db.UncasedSql("information_schema.tables");

            sql.RegisterFields("true");
            sql.Where($"table_name = '{_options.SchemaTable}'");

            if (!string.IsNullOrEmpty(_options.SchemaTableSchema))
                sql.AppendAnd($"table_schema = '{_options.SchemaTableSchema}'");

            var stmt = sql.ToSelect();
            var qry = _db.Query();

            return qry.FirstOrDefaultAsync<bool>(stmt);
        }

        public virtual Task<SchemaPhase> GetLastSchemaPhaseAsync()
        {
            var sql = new SqlBuilder(null, _options.SchemaTableSchema, _options.SchemaTable!);
            sql.OrderBy("version desc");

            var stmt = sql.ToSelect();
            var qry = _db.Query();

            return qry.FirstOrDefaultAsync<SchemaPhase>(stmt);
        }

        public virtual Task<IEnumerable<SchemaPhase>> GetSchemaPhasesAsync()
        {
            var sql = new SqlBuilder(null, _options.SchemaTableSchema, _options.SchemaTable!);
            sql.OrderBy("version");

            var stmt = sql.ToSelect();
            var qry = _db.Query();

            return qry.RetrieveAsync<SchemaPhase>(stmt);
        }

        public virtual Task InsertSchemaPhaseAsync(SchemaPhase phase)
        {
            var sql = new SqlBuilder(null, _options.SchemaTableSchema, _options.SchemaTable!);

            sql.RegisterFields(new[] { "version", "title", "description", "summary", "applied" });
            sql.RegisterValues(new[] { "@Version", "@Title", "@Description", "@Summary", "@Applied" });

            var stmt = sql.ToInsert();
            var qry = _db.Query();

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
}
