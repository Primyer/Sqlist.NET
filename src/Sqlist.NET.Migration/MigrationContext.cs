using System;
using System.Collections.Generic;
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
    /// <param name="roadmapProvider">The roadmap builder used for constructing migration roadmaps.</param>
    /// <param name="migrationTransaction">The transaction manager responsible for handling migration transactions.</param>
    /// <param name="migrationService">The migration service responsible for handling migration operations and scripts.</param>
    /// <param name="options">The migration options containing configuration settings for the migration process.</param>
    /// <param name="logger">The logger used for logging migration-related information and errors, if available.</param>
    internal class MigrationContext(
        IDbContext db,
        IMigrationService migrationService,
        IRoadmapProvider roadmapProvider,
        IMigrationTransactionManager migrationTransaction,
        IOptions<MigrationOptions> options,
        ILogger<MigrationContext> logger) : IMigrationContext
    {
        private readonly MigrationOptions _options = options.Value;

        private MigrationOperationInfo? _info;
        private DataTransactionMap? _datamap;

        [MemberNotNullWhen(true, nameof(_info))]
        private bool Initialized { get; set; }

        public MigrationOperationInfo? OperationInfo => _info;

        /// <inheritdoc />
        public async Task<MigrationOperationInfo> InitializeAsync(
            Version? targetVersion = null, CancellationToken cancellationToken = default)
        {
            LogInitialization(targetVersion);

            _info = await RetrieveSchemaDetailsAsync(cancellationToken);
            
            var datamap = await BuildTransactionMap(_options, _info, _info.CurrentVersion, targetVersion);
            var modules = await BuildModularMaps(_info.ModularMigrations);

            _datamap = DataTransactionMapMerger.SafeMerge(modules);
            if (_info.CurrentVersion is not null)
            {
                var phase = CreateSchemaTablePhase();
                _datamap.Merge(phase, _info.CurrentVersion);
            }
            
            DataTransactionMapMerger.FullMerge(datamap, _datamap);
            LogOperationInfo(_info, logger);
            
            Initialized = true;
            return _info;
        }

        private void LogInitialization(Version? targetVersion)
        {
            logger.LogInformation("Initializing migration...");
            logger.LogInformation("Target version: {version}", targetVersion?.ToString() ?? "Not specified");
        }

        private async Task<MigrationOperationInfo> RetrieveSchemaDetailsAsync(CancellationToken cancellationToken)
        {
            var info = new MigrationOperationInfo
            {
                CurrentVersion = await GetLatestVersionAsync(cancellationToken)
            };

            if (info.CurrentVersion is not null)
            {
                info.ModularMigrations = await GetModularRoadmapInfo(cancellationToken);
            }

            return info;
        }

        private async Task<Version?> GetLatestVersionAsync(CancellationToken cancellationToken)
        {
            var exists = await migrationService.DoesSchemaTableExistAsync(cancellationToken);
            if (!exists)
            {
                logger.LogDebug("No schema table could be located; the database is presumed empty.");
                return null;
            }

            var mainPhase = await migrationService.GetLastSchemaPhaseAsync(cancellationToken);
            if (mainPhase is null) return null;

            return new(mainPhase.Version);
        }

        private async Task<IReadOnlyDictionary<string, MigrationRoadmapInfo>> GetModularRoadmapInfo(
            CancellationToken cancellationToken)
        {
            if (_info?.CurrentVersion is null)
                return new Dictionary<string, MigrationRoadmapInfo>();

            var moduleInfo = new Dictionary<string, MigrationRoadmapInfo>();
            var modularPhases = await migrationService.GetModularSchemaPhasesAsync(cancellationToken);

            foreach (var phase in modularPhases)
            {
                if (phase.Package is null) continue;
                moduleInfo.Add(phase.Package, new MigrationRoadmapInfo { CurrentVersion = new Version(phase.Version) });
            }

            return moduleInfo;
        }

        private async Task<IEnumerable<DataTransactionMap>> BuildModularMaps(
            IReadOnlyDictionary<string, MigrationRoadmapInfo> modulesInfo)
        {
            var assets = _options.ModularAssets;
            var datamaps = new List<DataTransactionMap>(assets.Count);

            foreach (var (package, assetInfo) in assets)
            {
                if (!modulesInfo.TryGetValue(package, out var moduleInfo)) continue;

                var moduleMap = await BuildTransactionMap(assetInfo, moduleInfo, moduleInfo.CurrentVersion);
                datamaps.Add(moduleMap);
            }
            
            return datamaps.AsEnumerable();
        }

        private async Task<DataTransactionMap> BuildTransactionMap(MigrationAssetInfo assets, MigrationRoadmapInfo info,
            Version? currentVersion, Version? targetVersion = null)
        {
            var phases = await roadmapProvider.GetMigrationRoadmapAsync(assets, targetVersion);
            var datamap = new DataTransactionMap(phases, currentVersion);

            info.SetFromPhase(phases.Last(), datamap, targetVersion);
            return datamap;
        }

        private MigrationPhase CreateSchemaTablePhase()
        {
            var tableName = _options.SchemaTable ?? Consts.DefaultSchemaTable;
            var stringType = db.TypeMapper.TypeName<string>();
            var definition = new ColumnDefinition(stringType);

            var phase = new MigrationPhase();
            var columns = new DefinitionCollection
            {
                Columns =
                {
                    KeyValuePair.Create(Consts.Id,
                        new ColumnDefinition(db.TypeMapper.TypeName<int>()) { IsSequence = true }),
                    KeyValuePair.Create(Consts.Package, definition),
                    KeyValuePair.Create(Consts.Version, definition),
                    KeyValuePair.Create(Consts.Parent, new ColumnDefinition(db.TypeMapper.TypeName<int>())),
                    KeyValuePair.Create(Consts.Title, definition),
                    KeyValuePair.Create(Consts.Description, definition),
                    KeyValuePair.Create(Consts.Summary, definition),
                    KeyValuePair.Create(Consts.Applied, new ColumnDefinition(db.TypeMapper.TypeName<DateTime>())),
                }
            };

            phase.Guidelines.Create.Add(tableName, columns);
            return phase;
        }

        private static void LogOperationInfo(MigrationOperationInfo info, ILogger logger)
        {
            logger.LogInformation("* CORE MIGRATION INFO:");
            info.Log(logger);

            if (info.ModularMigrations.Count == 0) return;
            logger.LogInformation("* MODULAR MIGRATION INFO:");
            var count = 0;

            foreach (var (module, moduleInfo) in info.ModularMigrations)
            {
                logger.LogInformation("** {number}) {module}", ++count, module);
                moduleInfo.Log(logger);
            }
        }

        /// <inheritdoc />
        public async Task MigrateDataAsync(CancellationToken cancellationToken = default)
        {
            ValidateMigrationState();

            var dbname = db.Connection.Database;
            var oldName = dbname + "_" + DateTime.Now.Ticks;

            try
            {
                if (_info.CurrentVersion is not null)
                {
                    await migrationTransaction.PrepareDatabaseForMigrationAsync(dbname, oldName, cancellationToken);
                }

                await ExecuteMigrationsAsync(oldName, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error has occurred during migration; cleaning up...");
                await migrationTransaction.RollbackMigrationAsync(dbname, oldName, cancellationToken);

                if (ex is not MigrationException)
                    throw new MigrationException("An unexpected error occurred during migration.", ex);

                throw;
            }
        }

        [MemberNotNull(nameof(_info))]
        private void ValidateMigrationState()
        {
            if (string.IsNullOrEmpty(db.DefaultDatabase))
                throw new MigrationException(Resources.UnsupportedDatabase);

            if (!Initialized)
                throw new MigrationException(Resources.MigrationNotInitialized);
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

            logger.LogInformation("Data migration is successfully completed.");
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
                logger.LogInformation("Executing database scripts...");
                
                var resources = assets.ScriptsAssembly.GetEmbeddedResourcesAsync(assets.ScriptsPath);
                await foreach (var (resource, script) in resources)
                {
                    try
                    {
                        logger.LogDebug("Executing script resource: {resource}", resource);
                        await db.Query().ExecuteAsync(script!, cancellationToken: cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        throw new MigrationException(string.Format(Resources.ScriptExecutionFailed, resource), ex);
                    }
                }

                await migrationService.CreateSchemaTableAsync(cancellationToken);
                await db.CommitTransactionAsync(cancellationToken);

                logger.LogInformation("Database scripts are successfully executed.");
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
                logger.LogInformation("No previous version of the database was found; no data migration is required.");
            else
            {
                logger.LogInformation("Performing data migration...");
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