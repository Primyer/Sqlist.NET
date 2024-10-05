using System.Threading;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration;

/// <summary>
/// Provides an interface for managing the schema table used to track migration history.
/// </summary>
internal interface ISchemaTableManager
{
    /// <summary>
    /// Retrieves schema details, including the current schema version and modular migration information.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation, containing a <see cref="MigrationOperationInfo"/>
    /// object with the schema details.
    /// </returns>
    Task<MigrationOperationInfo> RetrieveSchemaDetailsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Gets the schema table definition as a <see cref="MigrationPhase"/>.
    /// </summary>
    /// <returns>A <see cref="MigrationPhase"/> representing the schema table definition.</returns>
    MigrationPhase GetSchemaTableDefinition();
}