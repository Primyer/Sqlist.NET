using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sqlist.NET.Data;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration.Infrastructure
{
    public class MigrationService
    {
        private const string Tab = "   ";

        private readonly DbContextBase _db;
        private readonly MigrationOptions _options;
        private readonly DbManager _dbTools;
        private readonly ILogger<MigrationService>? _logger;
        private bool _initialized;
        private DataTransactionMap? _dataMap;
        private MigrationOperationInformation? _info;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationService"/> class.
        /// </summary>
        [ActivatorUtilitiesConstructor]
        public MigrationService(DbContextBase db, IOptions<MigrationOptions> options, ILogger<MigrationService>? logger = null) : this(db, options?.Value, logger)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationService"/> class.
        /// </summary>
        public MigrationService(DbContextBase db, MigrationOptions? options, ILogger<MigrationService>? logger = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dbTools = new DbManager(_db, options);
            _logger = logger;
        }

        public MigrationOperationInformation? OperationInformation => _info;

        /// <summary>Initializes the migration service.</summary>
        /// <param name="version">The version that database is to be migrated up to.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="MigrationOperationInformation"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<MigrationOperationInformation> InitializeAsync(Version? version = null)
        {
            if (version is null)
                _logger?.LogInformation("Initializing migration"); else
                _logger?.LogInformation("Initializing migration to version {version}", version);

            _info = new MigrationOperationInformation();

            if (await _dbTools.DoesSchemaTableExistAsync())
            {
                var phase = await _dbTools.GetLastSchemaPhaseAsync();
                _info.CurrentVersion = new Version(phase.Version);
            }
            
            var dataMap = new DataTransactionMap();
            var roadMap = GetMigrationRoadMap()
                .Where(phase => version is null || phase.Version <= version)
                .OrderBy(phase => phase.Version);

            foreach (var phase in roadMap)
            {
                dataMap.Merge(phase, _info.CurrentVersion);
            }
            
            _dataMap = dataMap;
            _initialized = true;

            _info.SchemaChanges = GenerateSummary(_dataMap);
            _info.LatestVersion = roadMap.Last().Version;
            _info.TargetVersion = version != null && roadMap.LastOrDefault()?.Version == version ? version : _info.LatestVersion;

            TraceLogInformation(_info);
            return _info;
        }

        private void TraceLogInformation(MigrationOperationInformation info)
        {
            if (_logger is null)
                return;

            _logger.LogTrace("Current version: {version}", info.CurrentVersion);
            _logger.LogTrace("Migration to version: {version}", info.LatestVersion);
            _logger.LogTrace("Title: {title}", info.Title);
            _logger.LogTrace("Description: {description}", info.Description);
            _logger.LogTrace("Schema changes: {schema}", info.SchemaChanges);
        }

        private IEnumerable<MigrationPhase> GetMigrationRoadMap()
        {
            var deserializer = new MigrationDeserializer();
            var phasesList = new List<MigrationPhase>();
             
            _options.RoadmapAssembly?.ReadEmbeddedResources(_options.RoadmapPath, (_, content) =>
            {
                var phase = deserializer.DeserializePhase(content!);
                phasesList.Add(phase);
            });

            return phasesList;
        }

        private static string GenerateSummary(DataTransactionMap dataMap)
        {
            var sb = new StringBuilder();

            foreach (var (table, columns) in dataMap)
            {
                sb.AppendLine(table);

                foreach (var (name, rule) in columns)
                {
                    sb.Append(Tab + name + ": " + rule.Type);
                    
                    if (rule is DataTransactionRule)
                    {
                        if (!string.IsNullOrEmpty(rule.ColumnName))
                            sb.Append(" => " + rule.ColumnName);

                        if (!string.IsNullOrEmpty(rule.Cast))
                            sb.Append($", casted as ({rule.Cast})");
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Executes database migration.
        /// </summary>
        /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task MigrateDataAsync()
        {
            if (string.IsNullOrEmpty(_db.DefaultDatabase))
                throw new NotSupportedException("DBMS with no default database are not supported.");

            if (!_initialized)
                throw new MigrationException("Migration service has not been initialized.");

            var renamed = false;
            var created = false;

            var dbname = _db.Connection!.Database;
            var old_db = dbname + "_" + DateTime.Now.Ticks;

            try
            {
                await _db.Connection.ChangeDatabaseAsync(_db.DefaultDatabase);
                _db.TerminateDatabaseConnections(dbname);

                await _dbTools.RenameDatabaseAsync(dbname, old_db);
                renamed = true;

                await _dbTools.CreateDatabaseAsync(dbname);
                created = true;

                _logger?.LogInformation("Create new database.");

                await _db.Connection.ChangeDatabaseAsync(dbname);
                await _db.BeginTransactionAsync();

                try
                {
                    _logger?.LogInformation("Executing database scripts...");

                    await _options.ScriptsAssembly!.ReadEmbeddedResources(_options.ScriptsPath, async (resource, script) =>
                    {
                        try
                        {
                            await _db.Query().ExecuteAsync(script!);
                        }
                        catch (Exception ex)
                        {
                            throw new MigrationException($"Failed to execute scripts in resource '{resource}'.", ex);
                        }
                    });

                    await _dbTools.CreateSchemaTableAsync();
                    await _db.CommitTransactionAsync();

                    _logger?.LogInformation("Database scripts are successfully executed.");

                    await _db.BeginTransactionAsync();

                    if (_info!.CurrentVersion is null)
                        _logger?.LogInformation("No previous version of the database was found; no data migration required.");
                    else
                    {
                        _logger?.LogInformation("Performing data migration...");
                        await _dbTools.MigrateDataFromAsync(old_db, _dataMap!);
                    }

                    await _dbTools.InsertSchemaPhaseAsync(new SchemaPhase
                    {
                        Version = _info.LatestVersion?.ToString(),
                        Title = _info.Title,
                        Description = _info.Description,
                        Summary = _info.SchemaChanges
                    });

                    await _db.CommitTransactionAsync();
                    _logger?.LogInformation("Data migration is successfully completed");
                }
                catch (Exception)
                {
                    await _db.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception)
            {
                _logger?.LogError("An error has occurred during migration; cleaning up...");

                await _db.Connection.ChangeDatabaseAsync(_db.DefaultDatabase);
                _db.TerminateDatabaseConnections(dbname);

                if (created)
                    await _dbTools.DeleteDatabaseAsync(dbname);

                if (renamed)
                {
                    if (_info!.CurrentVersion != null)
                        _db.TerminateDatabaseConnections(old_db);

                    await _dbTools.RenameDatabaseAsync(old_db, dbname);
                }

                throw;
            }
        }
    }
}
