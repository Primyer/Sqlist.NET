using System;
using System.Collections.Generic;
using System.Linq;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration;

internal class RoadmapBuilder : IRoadmapBuilder
{
    public DataTransactionMap Build(
        ref IEnumerable<MigrationPhase> phases, Version? currentVersion, Version? targetVersion = null)
    {
        if (targetVersion is not null && targetVersion < currentVersion)
        {
            throw new MigrationException("The target version cannot be less than the current version.");
        }
        
        ValidateRoadmap(phases);
        
        phases = GetOrderedPhases(phases, targetVersion);
        var datamap = new DataTransactionMap(phases, currentVersion);

        return datamap;
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

    private static IEnumerable<MigrationPhase> GetOrderedPhases(IEnumerable<MigrationPhase> roadmap, Version? targetVersion)
    {
        return roadmap
            .Where(phase => targetVersion is null || phase.Version <= targetVersion)
            .OrderBy(phase => phase.Version);
    }
}