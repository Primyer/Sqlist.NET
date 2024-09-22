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
    internal class MigrationContext(IDbContext db, IMigrationService migrationService, IOptions<MigrationOptions> options, ILogger<MigrationContext>? logger = null) : IMigrationContext
    {
        private readonly MigrationOptions _options = options.Value;
        private readonly List<DataTransactionMap> _modularDatamaps = [];

        private MigrationOperationInfo? _info;
        private DataTransactionMap? _datamap;

        [MemberNotNullWhen(true, nameof(_info))]
        protected bool Initialized { get; private set; }

        [NotNullIfNotNull(nameof(_info))]
        public MigrationOperationInfo? OperationInfo => _info;

        /// <summary>Initializes the migration service.</summary>
        /// <param name="targetVersion">The version that database is to be migrated up to.</param>
        /// <returns>
        ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="MigrationOperationInfo"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<MigrationOperationInfo> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null, CancellationToken cancellationToken = default)
        {
            LogInitialization(targetVersion);
            await InitializeOperationAsync(cancellationToken);

            _datamap = BuildTransactionMap(_options, _info, _info.CurrentVersion ?? currentVersion, targetVersion);

            foreach (var (package, assets) in _options.ModularAssets)
            {
                if (!_info.ModularMigrations.TryGetValue(package, out var moduleInfo))
                    continue;

                var moduleMap = BuildTransactionMap(assets, moduleInfo, moduleInfo.CurrentVersion);
                _modularDatamaps.Add(moduleMap);
            }

            if (_info.CurrentVersion is null)
                _info.CurrentVersion = currentVersion;
            else
            {
                MergeSchemaDefinition();
            }

            Initialized = true;

            TraceLogInformation();
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
            if (targetVersion is null)
            {
                logger?.LogInformation("Initializing migration");
            }
            else
            {
                logger?.LogInformation("Initializing migration to version {version}", targetVersion);
            }
        }

        [MemberNotNull(nameof(_info))]
        private async Task InitializeOperationAsync(CancellationToken cancellationToken)
        {
            var moduleInfo = new Dictionary<string, MigrationRoadmapInfo>();
            _info = new()
            {
                ModularMigrations = moduleInfo
            };

            var exists = await migrationService.DoesSchemaTableExistAsync(cancellationToken);
            if (!exists) return;

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

                moduleInfo.Add(phase.Package, new() { CurrentVersion = new Version(phase.Version) });
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

        // TODO: Change to debug and fix the name.
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

        public static IList<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets)
        {
            var deserializer = new MigrationDeserializer();
            var phasesList = new List<MigrationPhase>();

            if (assets.RoadmapAssembly is null)
            {
                throw new MigrationException(string.Format(Resources.RoadmapAssemblyIsNull, nameof(MigrationAssetInfo.RoadmapAssembly)));
            }

            assets.RoadmapAssembly?.ReadEmbeddedResources(assets.RoadmapPath, (_, content) =>
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
        /// <exception cref="NotSupportedException" />
        /// <exception cref="MigrationException" />
        public virtual async Task MigrateDataAsync(CancellationToken cancellationToken = default)
        {
            ValidateMigrationState();

            var dbname = db.Connection.Database;
            var old_db = dbname + "_" + DateTime.Now.Ticks;

            try
            {
                await PrepareDatabaseForMigrationAsync(dbname, old_db, cancellationToken);
                await ExecuteMigrationsAsync(old_db, cancellationToken);
            }
            catch (MigrationException ex)
            {
                logger?.LogError(ex, "An error has occurred during migration; cleaning up...");
                await CleanupDatabaseAsync(dbname, old_db, ex, cancellationToken);
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError("An unexpected error occurred during migration; cleaning up...");
                await CleanupDatabaseAsync(dbname, old_db, ex, cancellationToken);

                throw new MigrationException("An unexpected error occurred during migration.", ex);
            }
        }

        private async Task ExecuteMigrationsAsync(string old_db, CancellationToken cancellationToken)
        {
            await ExecuteScriptsAsync(_options, cancellationToken);
            await ExecuteRoadmapAsync(old_db, cancellationToken);
            // TODO: Implement modular migrations.

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

        private async Task PrepareDatabaseForMigrationAsync(string dbname, string old_db, CancellationToken cancellationToken)
        {
            if (_info!.CurrentVersion is null)
                return;

            await db.TerminateDatabaseConnectionsAsync(dbname, cancellationToken);
            await migrationService.RenameDatabaseAsync(dbname, old_db, cancellationToken);
            await migrationService.CreateDatabaseAsync(dbname, cancellationToken);

            logger?.LogInformation("Created new database.");

            await db.Connection.ChangeDatabaseAsync(dbname, cancellationToken);
        }

        private async Task CleanupDatabaseAsync(string dbname, string old_db, Exception ex, CancellationToken cancellationToken)
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
                    await db.TerminateDatabaseConnectionsAsync(old_db, cancellationToken);
                    await migrationService.RenameDatabaseAsync(old_db, dbname, cancellationToken);
                }
            }
        }

        private async Task ExecuteScriptsAsync(MigrationAssetInfo assets, CancellationToken cancellationToken)
        {
            if (assets.ScriptsAssembly is null)
            {
                throw new MigrationException(string.Format(Resources.ScriptsAssemblyIsNull, nameof(MigrationAssetInfo.ScriptsAssembly)));
            }

            await db.BeginTransactionAsync(cancellationToken);

            try
            {
                logger?.LogInformation("Executing database scripts...");

                await assets.ScriptsAssembly.ReadEmbeddedResources(assets.ScriptsPath, async (resource, script) =>
                {
                    try
                    {
                        await db.Query().ExecuteAsync(script!);
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
                logger?.LogInformation("No previous version of the database was found; no data migration required.");
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
                Summary = _info.SchemaChanges
            };

            await migrationService.InsertSchemaPhaseAsync(phase, cancellationToken);
        }
    }
}
