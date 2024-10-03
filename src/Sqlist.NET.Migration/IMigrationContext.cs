using System;
using System.Threading;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Exceptions;

namespace Sqlist.NET.Migration;

/// <summary>
/// Represents the context for database migration operations.
/// This class is responsible for managing the migration process, including initialization,
/// execution of migration scripts, and handling data transactions.
/// </summary>
public interface IMigrationContext
{
    /// <summary>Initializes the migration service.</summary>
    /// <param name="targetVersion">The version that database is to be migrated up to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     The <see cref="Task"/> object that represents the asynchronous operation, containing the <see cref="MigrationOperationInfo"/>.
    /// </returns>
    Task<MigrationOperationInfo> InitializeAsync(Version? targetVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes database migration.
    /// </summary>
    /// <returns>The <see cref="Task"/> object that represents the asynchronous operation.</returns>
    /// <exception cref="MigrationException" />
    Task MigrateDataAsync(CancellationToken cancellationToken = default);
}