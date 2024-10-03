using System;
using System.Collections.Generic;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Infrastructure;

namespace Sqlist.NET.Migration;

internal interface IRoadmapProvider
{
    /// <summary>
    /// Builds a data transaction map based on the provided migration phases and version information.
    /// </summary>
    /// <param name="phases">The migration phases to be included in the roadmap.</param>
    /// <param name="currentVersion">The current version of the data schema.</param>
    /// <param name="targetVersion">The target version of the data schema. If null, the latest version is assumed.</param>
    /// <returns>A <see cref="DataTransactionMap"/> representing the roadmap for the migration.</returns>
    DataTransactionMap Build(IEnumerable<MigrationPhase> phases, Version? currentVersion, Version? targetVersion = null);

    /// <summary>
    /// Retrieves the migration roadmap from the specified assets.
    /// </summary>
    /// <param name="assets">The assets required for the migration, including scripts and roadmap information.</param>
    /// <param name="targetVersion">The target version of the data schema. If null, the latest version is assumed.</param>
    /// <returns>A list of <see cref="MigrationPhase"/> objects representing the migration roadmap.</returns>
    /// <exception cref="MigrationException">
    /// Thrown when the roadmap assembly is null or when there is an error during the deserialization of the roadmap.
    /// </exception>
    IEnumerable<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets, Version? targetVersion = null);
}