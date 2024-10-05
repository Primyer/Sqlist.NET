using System.Threading;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration;

internal interface ISchemaTableManager
{
    Task<MigrationOperationInfo> RetrieveSchemaDetailsAsync(CancellationToken cancellationToken);
    MigrationPhase GetSchemaTableDefinition();
}