using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlist.NET.Migration;
public interface IMigrationService
{
    Task CreateDatabaseAsync(string database, CancellationToken cancellationToken);
    Task CreateSchemaTableAsync(CancellationToken cancellationToken);
    Task DeleteDatabaseAsync(string database, CancellationToken cancellationToken);
    Task<bool> DoesSchemaTableExistAsync(CancellationToken cancellationToken);
    Task<SchemaPhase> GetLastSchemaPhaseAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SchemaPhase>> GetModularSchemaPhasesAsync(CancellationToken cancellationToken);
    Task<int> InsertSchemaPhaseAsync(SchemaPhase phase, CancellationToken cancellationToken);
    Task MigrateDataFromAsync(string dbname, DataTransactionMap dataMap, CancellationToken cancellationToken);
    Task RenameDatabaseAsync(string currentName, string newName, CancellationToken cancellationToken);
    Task InsertSchemaPhasesAsync(IEnumerable<SchemaPhase> phases, CancellationToken cancellationToken);
}
