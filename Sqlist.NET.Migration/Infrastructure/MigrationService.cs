using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Properties;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration.Infrastructure
{
    public class MigrationService
    {
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
                _info.CurrentVersion = new Version(phase.Version!);
            }
            
            var roadMap = GetMigrationRoadMap()
                .Where(phase => version is null || phase.Version <= version)
                .OrderBy(phase => phase.Version);

            _dataMap = new DataTransactionMap(roadMap, _info.CurrentVersion);
            MergeSchemaDefinition();

            var lastPhase = roadMap.Last();

            _info.Title = lastPhase.Title;
            _info.Description = lastPhase.Description;
            _info.SchemaChanges = _dataMap.GenerateSummary();
            _info.LatestVersion = lastPhase.Version;
            _info.TargetVersion = version != null && roadMap.LastOrDefault()?.Version == version ? version : _info.LatestVersion;

            _initialized = true;

            TraceLogInformation();
            return _info;
        }

        private void MergeSchemaDefinition()
        {
            var strType = _db.TypeMapper.TypeName<string>();
            var definition = new ColumnDefinition(strType);

            var phase = new MigrationPhase()
            {
                Guidelines =
                {
                    Create =
                    {
                        [_options.SchemaTable!] = new DefinitionCollection()
                        {
                            Columns =
                            {
                                KeyValuePair.Create(Consts.Version, definition),
                                KeyValuePair.Create(Consts.Title, definition),
                                KeyValuePair.Create(Consts.Description, definition),
                                KeyValuePair.Create(Consts.Summary, definition),
                                KeyValuePair.Create(Consts.Applied, new ColumnDefinition(_db.TypeMapper.TypeName<DateTime>())),
                            }
                        }
                    }
                }
            };

            _dataMap?.Merge(phase, _info!.CurrentVersion);
        }

        private void TraceLogInformation()
        {
            if (_logger is null)
                return;

            _logger.LogTrace("Current version: {version}", _info!.CurrentVersion);
            _logger.LogTrace("Migration to version: {version}", _info.LatestVersion);
            _logger.LogTrace("Title: {title}", _info.Title);
            _logger.LogTrace("Description: {description}", _info.Description);
            _logger.LogTrace("Schema changes: {schema}", _info.SchemaChanges);
        }

        public IEnumerable<MigrationPhase> GetMigrationRoadMap()
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
                if (_info!.CurrentVersion is not null)
                {
                    await _db.TerminateDatabaseConnectionsAsync(dbname);
                    await _dbTools.RenameDatabaseAsync(dbname, old_db);
                    renamed = true;

                    await _dbTools.CreateDatabaseAsync(dbname);
                    created = true;

                    _logger?.LogInformation("Created new database.");

                    await _db.Connection!.ChangeDatabaseAsync(dbname);
                }

                await ExecuteScriptsAsync();
                await ExecuteMigrationAsync(old_db);
            }
            catch (Exception)
            {
                _logger?.LogError("An error has occurred during migration; cleaning up...");

                if (_db.Connection.State != ConnectionState.Open)
                    await _db.Connection.OpenAsync();

                if (_info!.CurrentVersion is not null)
                {
                    await _db.TerminateDatabaseConnectionsAsync(dbname);

                    if (created)
                        await _dbTools.DeleteDatabaseAsync(dbname);

                    if (renamed)
                    {
                        await _db.TerminateDatabaseConnectionsAsync(old_db);
                        await _dbTools.RenameDatabaseAsync(old_db, dbname);
                    }
                }

                throw;
            }
        }

        private async Task ExecuteScriptsAsync()
        {
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
            }
            catch (Exception)
            {
                await _db.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task ExecuteMigrationAsync(string dbname)
        {
            if (_info!.CurrentVersion is null)
                _logger?.LogInformation("No previous version of the database was found; no data migration required.");
            else
            {
                _logger?.LogInformation("Performing data migration...");
                await _dbTools.MigrateDataFromAsync(dbname, _dataMap!);
            }

            await _dbTools.InsertSchemaPhaseAsync(new SchemaPhase
            {
                Version = _info.LatestVersion?.ToString(),
                Title = _info.Title,
                Description = _info.Description,
                Summary = _info.SchemaChanges
            });

            _logger?.LogInformation("Data migration is successfully completed.");
        }
    }
}
