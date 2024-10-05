using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Infrastructure;

namespace Sqlist.NET.Migration;

internal interface IRoadmapProvider
{
    /// <summary>
    /// Retrieves the migration roadmap from the specified assets.
    /// </summary>
    /// <param name="assets">The assets required for the migration, including scripts and roadmap information.</param>
    /// <param name="targetVersion">The target version of the data schema. If null, the latest version is assumed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// The <see cref="Task"/> object that represents the asynchronous operation, containing a list of
    /// <see cref="MigrationPhase"/> objects representing the migration roadmap.
    /// </returns>
    /// <exception cref="MigrationException">
    /// Thrown when the roadmap assembly is null or when there is an error during the deserialization of the roadmap.
    /// </exception>
    Task<IEnumerable<MigrationPhase>> GetMigrationRoadmapAsync(
        MigrationAssetInfo assets, Version? targetVersion = null, CancellationToken cancellationToken = default);
}