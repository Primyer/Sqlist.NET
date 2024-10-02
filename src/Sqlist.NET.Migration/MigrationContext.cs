using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration
{
    /// <inheritdoc cref="IMigrationContext"/>
    /// <param name="db">The database context used for executing database operations.</param>
    /// <param name="migrationService">The migration service responsible for handling migration operations and scripts.</param>
    /// <param name="options">The migration options containing configuration settings for the migration process.</param>
    /// <param name="logger">The logger used for logging migration-related information and errors, if available.</param>
    internal class MigrationContext(
        IDbContext db,
        IMigrationService migrationService,
        IOptions<MigrationOptions> options,
        ILogger<MigrationContext>? logger = null) : IMigrationContext
    {
        private readonly MigrationOptions _options = options.Value;

        private MigrationOperationInfo? _info;
        private DataTransactionMap? _datamap;

        [MemberNotNullWhen(true, nameof(_info))]
        private bool Initialized { get; set; }

        [NotNullIfNotNull(nameof(_info))]
        public MigrationOperationInfo? OperationInfo => _info;

        /// <inheritdoc />
        public async Task<MigrationOperationInfo> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null, CancellationToken cancellationToken = default)
        {
            LogInitialization(targetVersion);
            await InitializeOperationAsync(cancellationToken);

            _datamap = BuildTransactionMap(_options, _info, _info.CurrentVersion ?? currentVersion, targetVersion);
            var modules = new List<DataTransactionMap>();
            
            foreach (var (package, assets) in _options.ModularAssets)
            {
                if (!_info.ModularMigrations.TryGetValue(package, out var moduleInfo))
                    continue;

                var moduleMap = BuildTransactionMap(assets, moduleInfo, moduleInfo.CurrentVersion);
                modules.Add(moduleMap);
            }

            if (_info.CurrentVersion is null)
                _info.CurrentVersion = currentVersion;
            else
            {
                MergeSchemaDefinition();
            }

            var modularRoadmap = DataTransactionMapMerger.SafeMerge(modules);
            DataTransactionMapMerger.FullMerge(_datamap, modularRoadmap);

            Initialized = true;

            LogOperationInfo();
            return _info;
        }

        private static DataTransactionMap BuildTransactionMap(MigrationAssetInfo assets, MigrationRoadmapInfo info, Version? currentVersion, Version? targetVersion = null)
        {
            var roadmap = GetMigrationRoadmap(assets);
            ValidateRoadmap(roadmap);

            var phases = GetOrderedPhases(roadmap, targetVersion);
            var datamap = new DataTransactionMap(phases, currentVersion);

            SetOperationInformation(info, datamap, phases.Last(), targetVersion);

            return datamap;
        }

        private void LogInitialization(Version? targetVersion)
        {
            logger?.LogInformation("Initializing migration...");
            logger?.LogInformation("Target version: {version}", targetVersion?.ToString() ?? "Not specified");
        }

        [MemberNotNull(nameof(_info))]
        private async Task InitializeOperationAsync(CancellationToken cancellationToken)
        {
            var moduleInfo = new Dictionary<string, MigrationRoadmapInfo>();
            _info = new MigrationOperationInfo
            {
                ModularMigrations = moduleInfo
            };

            var exists = await migrationService.DoesSchemaTableExistAsync(cancellationToken);
            if (!exists)
            {
                logger?.LogDebug("No schema table could be located; the database is presumed empty.");
                return;
            }

            var mainPhase = await migrationService.GetLastSchemaPhaseAsync(cancellationToken);
            if (mainPhase is not null)
            {
                _info.CurrentVersion = new Version(mainPhase.Version);
            }

            var modularPhases = await migrationService.GetModularSchemaPhasesAsync(cancellationToken);
            
            foreach (var phase in modularPhases)
            {
                if (phase.Package is null)
                    continue;

                moduleInfo.Add(phase.Package, new MigrationRoadmapInfo { CurrentVersion = new Version(phase.Version) });
            }
        }

        private static void ValidateRoadmap(IList<MigrationPhase> roadmap)
        {
            if (roadmap.Count == 0)
            {
                throw new MigrationException(Resources.EmptyRoadmap);
            }
        }

        private static IEnumerable<MigrationPhase> GetOrderedPhases(IList<MigrationPhase> roadmap, Version? targetVersion)
        {
            return roadmap
                .Where(phase => targetVersion is null || phase.Version <= targetVersion)
                .OrderBy(phase => phase.Version);
        }

        private static void SetOperationInformation(MigrationRoadmapInfo info, DataTransactionMap datamap, MigrationPhase lastPhase, Version? targetVersion)
        {
            info.Title = lastPhase.Title;
            info.Description = lastPhase.Description;
            info.SchemaChanges = datamap.GenerateSummary();
            info.LatestVersion = lastPhase.Version;
            info.TargetVersion = targetVersion != null && lastPhase.Version == targetVersion ? targetVersion : info.LatestVersion;
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
                        [_options.SchemaTable ?? Consts.DefaultSchemaTable] = new DefinitionCollection()
                        {
                            Columns =
                            {
                                KeyValuePair.Create(Consts.Id, new ColumnDefinition(db.TypeMapper.TypeName<int>()) { IsSequence = true }),
                                KeyValuePair.Create(Consts.Package, definition),
                                KeyValuePair.Create(Consts.Version, definition),
                                KeyValuePair.Create(Consts.Parent, new ColumnDefinition(db.TypeMapper.TypeName<int>())),
                                KeyValuePair.Create(Consts.Title, definition),
                                KeyValuePair.Create(Consts.Description, definition),
                                KeyValuePair.Create(Consts.Summary, definition),
                                KeyValuePair.Create(Consts.Applied, new ColumnDefinition(db.TypeMapper.TypeName<DateTime>())),
                            }
                        }
                    }
                }
            };

            _datamap?.Merge(phase, _info!.CurrentVersion);
        }

        private void LogOperationInfo()
        {
            if (_info is null)
            {
                throw new InvalidOperationException("MigrationOperationInfo is unexpectedly null.");
            }
            
            if (logger is null) return;
            logger.LogInformation("* CORE MIGRATION INFO:");
            _info.Log(logger);

            if (_info.ModularMigrations.Count == 0) return;
            logger.LogInformation("* MODULAR MIGRATION INFO:");
            var count = 0;

            foreach (var (module, moduleInfo) in _info.ModularMigrations)
            {
                logger.LogInformation("** {number}) {module}", ++count, module);
                moduleInfo.Log(logger);
            }
        }

        /// <inheritdoc />
        public static IList<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets)
        {
            var deserializer = new MigrationDeserializer();
            var phasesList = new List<MigrationPhase>();

            if (assets.RoadmapAssembly is null)
            {
                throw new MigrationException(
                    string.Format(Resources.RoadmapAssemblyIsNull, nameof(MigrationAssetInfo.RoadmapAssembly)));
            }

            assets.RoadmapAssembly?.ReadEmbeddedResources(assets.RoadmapPath, (_, content) =>
            {
                var phase = deserializer.DeserializePhase(content!);
                phasesList.Add(phase);
            });

            return phasesList;
        }

        /// <inheritdoc />
        public async Task MigrateDataAsync(CancellationToken cancellationToken = default)
        {
            ValidateMigrationState();

            var dbname = db.Connection.Database;
            var oldDb = dbname + "_" + DateTime.Now.Ticks;

            try
            {
                // Prepare the database for migration by renaming the current database and creating a new one
                await PrepareDatabaseForMigrationAsync(dbname, oldDb, cancellationToken); 
                await ExecuteMigrationsAsync(oldDb, cancellationToken); // Execute the migration scripts and roadmap
            }
            catch (MigrationException ex)
            {
                // Log the error and clean up the database in case of a migration-specific exception
                logger?.LogError(ex, "An error has occurred during migration; cleaning up...");
                await CleanupDatabaseAsync(dbname, oldDb, ex, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                // Log the error and clean up the database
                logger?.LogError("An unexpected error occurred during migration; cleaning up...");
                await CleanupDatabaseAsync(dbname, oldDb, ex, cancellationToken);

                throw new MigrationException("An unexpected error occurred during migration.", ex);
            }
        }
        
        /// <summary>
        /// Executes the migration operations, including executing scripts and the migration roadmap.
        /// </summary>
        /// <param name="oldDb">The name of the old database to be used during migration.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous migration operation.</returns>
        /// <exception cref="MigrationException">Thrown when an error occurs during migration.</exception>
        private async Task ExecuteMigrationsAsync(string oldDb, CancellationToken cancellationToken)
        {
            await ExecuteScriptsAsync(_options, cancellationToken);
            await ExecuteRoadmapAsync(oldDb, cancellationToken);

            await RecordMigrationPhaseAsync(cancellationToken);

            logger?.LogInformation("Data migration is successfully completed.");
        }

        private void ValidateMigrationState()
        {
            if (string.IsNullOrEmpty(db.DefaultDatabase))
                throw new MigrationException(Resources.UnsupportedDatabase);

            if (!Initialized)
                throw new MigrationException(Resources.MigrationNotInitialized);
        }

        /// <summary>
        /// Prepares the database for migration by renaming the current database and creating a new one.
        /// </summary>
        /// <param name="dbname">The name of the new database.</param>
        /// <param name="oldDb">The name of the old database to be renamed.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        private async Task PrepareDatabaseForMigrationAsync(string dbname, string oldDb, CancellationToken cancellationToken)
        {
            if (_info!.CurrentVersion is null)
                return;

            await db.TerminateDatabaseConnectionsAsync(dbname, cancellationToken);
            await migrationService.RenameDatabaseAsync(dbname, oldDb, cancellationToken);
            await migrationService.CreateDatabaseAsync(dbname, cancellationToken);

            logger?.LogInformation("Created a new database.");
            logger?.LogDebug("""The old database was renamed to "{name}".""", oldDb);

            await db.Connection.ChangeDatabaseAsync(dbname, cancellationToken);
        }

        /// <summary>
        /// Cleans up the database in case of an error during migration.
        /// </summary>
        /// <param name="dbname">The name of the new database.</param>
        /// <param name="oldDb">The name of the old database.</param>
        /// <param name="ex">The exception that occurred during migration.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous cleanup operation.</returns>
        private async Task CleanupDatabaseAsync(string dbname, string oldDb, Exception ex, CancellationToken cancellationToken)
        {
            if (db.Connection.State != ConnectionState.Open)
            {
                await db.Connection.OpenAsync(cancellationToken);
            }

            if (_info!.CurrentVersion is not null)
            {
                await db.TerminateDatabaseConnectionsAsync(dbname, cancellationToken);

                if (ex is MigrationException)
                {
                    await migrationService.DeleteDatabaseAsync(dbname, cancellationToken);
                    await db.TerminateDatabaseConnectionsAsync(oldDb, cancellationToken);
                    await migrationService.RenameDatabaseAsync(oldDb, dbname, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Executes the migration scripts provided in the assets.
        /// </summary>
        /// <param name="assets">The assets required for the migration, including scripts and roadmap information.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <exception cref="MigrationException">
        /// Thrown when the scripts assembly is null or when there is an error during script execution.
        /// </exception>
        /// <remarks>
        /// This method reads the embedded resources from the scripts assembly and executes each script
        /// against the database. If any script fails, the transaction is rolled back and an exception is thrown.
        /// </remarks>
        private async Task ExecuteScriptsAsync(MigrationAssetInfo assets, CancellationToken cancellationToken)
        {
            if (assets.ScriptsAssembly is null)
            {
                throw new MigrationException(
                    string.Format(Resources.ScriptsAssemblyIsNull, nameof(MigrationAssetInfo.ScriptsAssembly)));
            }

            await db.BeginTransactionAsync(cancellationToken);
            try
            {
                logger?.LogInformation("Executing database scripts...");

                // Read and execute each script from the embedded resources in the scripts assembly
                await assets.ScriptsAssembly.ReadEmbeddedResources(assets.ScriptsPath, async (resource, script) =>
                {
                    try
                    {
                        logger?.LogDebug("Executing script resource: {resource}", resource);
                        await db.Query().ExecuteAsync(script!, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new MigrationException(string.Format(Resources.ScriptExecutionFailed, resource), ex);
                    }
                });

                await migrationService.CreateSchemaTableAsync(cancellationToken);
                await db.CommitTransactionAsync(cancellationToken);

                logger?.LogInformation("Database scripts are successfully executed.");
            }
            catch (Exception)
            {
                await db.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private async Task ExecuteRoadmapAsync(string dbname, CancellationToken cancellationToken)
        {
            if (_info!.CurrentVersion is null)
                logger?.LogInformation("No previous version of the database was found; no data migration is required.");
            else
            {
                logger?.LogInformation("Performing data migration...");
                await migrationService.MigrateDataFromAsync(dbname, _datamap!, cancellationToken);
            }
        }

        private async Task RecordMigrationPhaseAsync(CancellationToken cancellationToken)
        {
            var phase = new SchemaPhase
            {
                Version = _info!.LatestVersion.ToString(),
                Title = _info.Title,
                Description = _info.Description,
                Summary = _info.SchemaChanges,
                
            };

            await migrationService.InsertSchemaPhaseAsync(phase, cancellationToken);
        }
    }
}
