using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration;

/// <summary>
/// Provides an interface for managing database operations related to migrations, 
/// with a focus on the transactional aspect of the process.
/// </summary>
internal interface IMigrationTransactionManager
{
    /// <summary>
    /// Prepares the database for migration by checking for a previous version, terminating connections,
    /// renaming the existing database, creating a new database, and switching the connection to the new database.
    /// </summary>
    /// <param name="dbname">The name of the database to be migrated.</param>
    /// <param name="oldName">The name to use for the old database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PrepareDatabaseForMigrationAsync(string dbname, string oldName, CancellationToken cancellationToken);
    
    /// <summary>
    /// Rolls back the migration by performing cleanup actions after a failed migration.
    /// This typically involves deleting the new database and restoring the old database if it exists.
    /// </summary>
    /// <param name="dbname">The name of the new database.</param>
    /// <param name="oldName">The name of the old database.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task RollbackMigrationAsync(string dbname, string oldName, CancellationToken cancellationToken);
}