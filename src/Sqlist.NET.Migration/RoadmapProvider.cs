using System;
using System.Collections.Generic;
using System.Linq;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration;

internal class RoadmapProvider : IRoadmapProvider
{
    /// <inheritdoc />
    public IEnumerable<MigrationPhase> GetMigrationRoadmap(MigrationAssetInfo assets, Version? targetVersion = null)
    {
        var deserializer = new MigrationDeserializer();
        var phasesList = new List<MigrationPhase>();

        if (assets.RoadmapAssembly is null)
        {
            var errorMessage =
                string.Format(Resources.RoadmapAssemblyIsNull, nameof(MigrationAssetInfo.RoadmapAssembly));
            throw new MigrationException(errorMessage);
        }

        assets.RoadmapAssembly?.ReadEmbeddedResources(assets.RoadmapPath, (_, content) =>
        {
            var phase = deserializer.DeserializePhase(content!);
            phasesList.Add(phase);
        });

        return OrderPhasesByVersion(phasesList, targetVersion);
    }

    /// <inheritdoc />
    public DataTransactionMap Build(
        IEnumerable<MigrationPhase> phases, Version? currentVersion, Version? targetVersion = null)
    {
        if (targetVersion is not null && targetVersion < currentVersion)
        {
            throw new MigrationException("The target version cannot be less than the current version.");
        }

        ValidateRoadmap(phases);
        return new DataTransactionMap(phases, currentVersion);
    }

    private static void ValidateRoadmap(IEnumerable<MigrationPhase> roadmap)
    {
        if (!roadmap.Any())
        {
            throw new MigrationException(Resources.EmptyRoadmap);
        }

        var duplicate = roadmap
            .GroupBy(p => p.Version)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .FirstOrDefault();

        if (duplicate is not null)
        {
            throw new MigrationException("The roadmap contains duplicate versions: " + duplicate);
        }
    }

    private static IOrderedEnumerable<MigrationPhase> OrderPhasesByVersion(
        IEnumerable<MigrationPhase> roadmap, Version? targetVersion)
    {
        return roadmap.Where(phase => targetVersion is null || phase.Version <= targetVersion)
            .OrderBy(phase => phase.Version);
    }
}