using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration;

internal class RoadmapProvider : IRoadmapProvider
{
    /// <inheritdoc />
    public async Task<IEnumerable<MigrationPhase>> GetMigrationRoadmapAsync(
        MigrationAssetInfo assets, Version? targetVersion = null)
    {
        var deserializer = new MigrationDeserializer();
        var phasesList = new List<MigrationPhase>();

        if (assets.RoadmapAssembly is null)
        {
            var errorMessage = string.Format(Resources.RoadmapAssemblyIsNull, nameof(MigrationAssetInfo.RoadmapAssembly));
            throw new MigrationException(errorMessage);
        }

        var resources = assets.RoadmapAssembly.GetEmbeddedResourcesAsync(assets.RoadmapPath);
        await foreach (var (_, content) in resources)
        {
            var phase = deserializer.DeserializePhase(content!);
            phasesList.Add(phase);
        }
        
        return OrderPhasesByVersion(phasesList, targetVersion);
    }
    
    private static IOrderedEnumerable<MigrationPhase> OrderPhasesByVersion(
        IEnumerable<MigrationPhase> roadmap, Version? targetVersion)
    {
        return roadmap.Where(phase => targetVersion is null || phase.Version <= targetVersion)
            .OrderBy(phase => phase.Version);
    }
}