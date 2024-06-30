using Sqlist.NET.Migration.Data;

using System.Threading.Tasks;

namespace Sqlist.NET.Migration;
public interface IMigrationService
{
    Task CreateDatabaseAsync(string database);
    Task CreateSchemaTableAsync();
    Task DeleteDatabaseAsync(string database);
    Task<bool> DoesSchemaTableExistAsync();
    Task<SchemaPhase> GetLastSchemaPhaseAsync();
    Task InsertSchemaPhaseAsync(SchemaPhase phase);
    Task MigrateDataFromAsync(string dbname, DataTransactionMap dataMap);
    Task RenameDatabaseAsync(string currentName, string newName);
}
