using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Sqlist.NET.Infrastructure;

namespace Sqlist.NET.Migration;

/// <inheritdoc cref="IMigrationTransactionManager"/>
/// <param name="db">The database context used for executing database operations.</param>
/// <param name="migrationService">The migration service responsible for handling migration operations and scripts.</param>
/// <param name="logger">The logger used for logging migration-related information and errors, if available.</param>
internal class MigrationTransactionManager(
    IDbContext db,
    IMigrationService migrationService,
    ILogger<MigrationTransactionManager> logger)
    : IMigrationTransactionManager
{
    private bool _renamed;
    private bool _created;
    
    /// <inheritdoc />
    public async Task PrepareDatabaseForMigrationAsync(string dbname, string oldName,
        CancellationToken cancellationToken)
    {
        await db.TerminateDatabaseConnectionsAsync(dbname, cancellationToken);
        
        await migrationService.RenameDatabaseAsync(dbname, oldName, cancellationToken);
        _renamed = true; // Mark the database as renamed to avoid issues during rollback
        
        await migrationService.CreateDatabaseAsync(dbname, cancellationToken);
        _created = true; // Mark the database as created to avoid issues during rollback

        logger.LogInformation("Created a new database.");
        logger.LogDebug("""The old database was renamed to "{name}".""", oldName);

        await db.Connection.ChangeDatabaseAsync(dbname, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackMigrationAsync(string dbname, string oldName, CancellationToken cancellationToken)
    {
        if (db.Connection.State != ConnectionState.Open)
        {
            await db.Connection.OpenAsync(cancellationToken);
        }

        // TODO: Gracefully handle exceptions during rollback steps
        await db.TerminateDatabaseConnectionsAsync(dbname, cancellationToken);
        if (_created)
        {
            await migrationService.DeleteDatabaseAsync(dbname, cancellationToken);
            logger.LogInformation("Deleted the new database.");
        }
        if (_renamed)
        {
            await db.TerminateDatabaseConnectionsAsync(oldName, cancellationToken);
            await migrationService.RenameDatabaseAsync(oldName, dbname, cancellationToken);
            logger.LogInformation("Restored the old database.");
        }
    }
}