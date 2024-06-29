using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration
{
    public class MigrationContext(DbContextBase db, MigrationService migrationService, IOptions<MigrationOptions> options, ILogger<MigrationContext>? logger = null)
    {
        private readonly MigrationOptions _options = options.Value;

        private bool _initialized;
        private DataTransactionMap? _dataMap;
        private MigrationOperationInformation? _info;

        public MigrationOperationInformation? OperationInformation => _info;

        /// <summary>Initializes the migration service.</summary>
        /// <param name="targetVersion">The version that database is to be migrated up to.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="MigrationOperationInformation"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<MigrationOperationInformation> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null)
        {
            if (targetVersion is null)
                logger?.LogInformation("Initializing migration");
            else
                logger?.LogInformation("Initializing migration to version {version}", targetVersion);

            _info = new MigrationOperationInformation();

            if (await migrationService.DoesSchemaTableExistAsync())
            {
                var phase = await migrationService.GetLastSchemaPhaseAsync();
                _info.CurrentVersion = new Version(phase.Version!);
            }

            var roadMap = GetMigrationRoadMap()
                .Where(phase => targetVersion is null || phase.Version <= targetVersion)
                .OrderBy(phase => phase.Version);

            _dataMap = new DataTransactionMap(roadMap, _info.CurrentVersion ?? currentVersion);

            if (_info.CurrentVersion is null)
                _info.CurrentVersion = currentVersion;
            else
                MergeSchemaDefinition();

            var lastPhase = roadMap.Last();

            _info.Title = lastPhase.Title;
            _info.Description = lastPhase.Description;
            _info.SchemaChanges = _dataMap.GenerateSummary();
            _info.LatestVersion = lastPhase.Version;
            _info.TargetVersion = targetVersion != null && roadMap.LastOrDefault()?.Version == targetVersion ? targetVersion : _info.LatestVersion;

            _initialized = true;

            TraceLogInformation();
            return _info;
        }

        private void MergeSchemaDefinition()
        {
            var strType = db.TypeMapper.TypeName<string>();
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
                                KeyValuePair.Create(Consts.Applied, new ColumnDefinition(db.TypeMapper.TypeName<DateTime>())),
                            }
                        }
                    }
                }
            };

            _dataMap?.Merge(phase, _info!.CurrentVersion);
        }

        private void TraceLogInformation()
        {
            if (logger is null)
                return;

            logger.LogTrace("Current version: {version}", _info!.CurrentVersion);
            logger.LogTrace("Migration to version: {version}", _info.LatestVersion);
            logger.LogTrace("Title: {title}", _info.Title);
            logger.LogTrace("Description: {description}", _info.Description);
            logger.LogTrace("Schema changes: {schema}", _info.SchemaChanges);
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
            if (string.IsNullOrEmpty(db.DefaultDatabase))
                throw new NotSupportedException("DBMS with no default database are not supported.");

            if (!_initialized)
                throw new MigrationException("Migration service has not been initialized.");

            var renamed = false;
            var created = false;

            var dbname = db.Connection!.Database;
            var old_db = dbname + "_" + DateTime.Now.Ticks;

            try
            {
                if (_info!.CurrentVersion is not null)
                {
                    await db.TerminateDatabaseConnectionsAsync(dbname);
                    await migrationService.RenameDatabaseAsync(dbname, old_db);
                    renamed = true;

                    await migrationService.CreateDatabaseAsync(dbname);
                    created = true;

                    logger?.LogInformation("Created new database.");

                    await db.Connection!.ChangeDatabaseAsync(dbname);
                }

                await ExecuteScriptsAsync();
                await ExecuteMigrationAsync(old_db);
            }
            catch (Exception)
            {
                logger?.LogError("An error has occurred during migration; cleaning up...");

                if (db.Connection.State != ConnectionState.Open)
                    await db.Connection.OpenAsync();

                if (_info!.CurrentVersion is not null)
                {
                    await db.TerminateDatabaseConnectionsAsync(dbname);

                    if (created)
                        await migrationService.DeleteDatabaseAsync(dbname);

                    if (renamed)
                    {
                        await db.TerminateDatabaseConnectionsAsync(old_db);
                        await migrationService.RenameDatabaseAsync(old_db, dbname);
                    }
                }

                throw;
            }
        }

        private async Task ExecuteScriptsAsync()
        {
            await db.BeginTransactionAsync();

            try
            {
                logger?.LogInformation("Executing database scripts...");

                await _options.ScriptsAssembly!.ReadEmbeddedResources(_options.ScriptsPath, async (resource, script) =>
                {
                    try
                    {
                        await db.Query().ExecuteAsync(script!);
                    }
                    catch (Exception ex)
                    {
                        throw new MigrationException($"Failed to execute scripts in resource '{resource}'.", ex);
                    }
                });

                await migrationService.CreateSchemaTableAsync();
                await db.CommitTransactionAsync();

                logger?.LogInformation("Database scripts are successfully executed.");
            }
            catch (Exception)
            {
                await db.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task ExecuteMigrationAsync(string dbname)
        {
            if (_info!.CurrentVersion is null)
                logger?.LogInformation("No previous version of the database was found; no data migration required.");
            else
            {
                logger?.LogInformation("Performing data migration...");
                await migrationService.MigrateDataFromAsync(dbname, _dataMap!);
            }

            await migrationService.InsertSchemaPhaseAsync(new SchemaPhase
            {
                Version = _info.LatestVersion?.ToString(),
                Title = _info.Title,
                Description = _info.Description,
                Summary = _info.SchemaChanges
            });

            logger?.LogInformation("Data migration is successfully completed.");
        }
    }
}
