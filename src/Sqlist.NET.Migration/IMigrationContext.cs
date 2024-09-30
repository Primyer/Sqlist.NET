using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Exceptions;

namespace Sqlist.NET.Migration;

/// <summary>
/// Represents the context for database migration operations.
/// This class is responsible for managing the migration process, including initialization,
/// execution of migration scripts, and handling data transactions.
/// </summary>
public interface IMigrationContext
{
    /// <summary>
    /// Retrieves the migration roadmap from the specified assets.
    /// </summary>
    /// <param name="assets">The assets required for the migration, including scripts and roadmap information.</param>
    /// <returns>A list of <see cref="MigrationPhase"/> objects representing the migration roadmap.</returns>
    /// <exception cref="MigrationException">
    /// Thrown when the roadmap assembly is null or when there is an error during the deserialization of the roadmap.
    /// </exception>
    static virtual IList<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets) => throw new NotImplementedException();
    
    /// <summary>Initializes the migration service.</summary>
    /// <param name="targetVersion">The version that database is to be migrated up to.</param>
    /// <param name="currentVersion">The current version of the database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="MigrationOperationInfo"/>.
    /// </returns>
    Task<MigrationOperationInfo> InitializeAsync(Version? targetVersion = null, Version? currentVersion = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    ///     Executes database migration.
    /// </summary>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    /// <exception cref="MigrationException" />
    Task MigrateDataAsync(CancellationToken cancellationToken = default);
}
