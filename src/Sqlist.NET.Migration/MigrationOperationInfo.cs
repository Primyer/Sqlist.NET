using System.Collections.Generic;

namespace Sqlist.NET.Migration;
public class MigrationOperationInfo : MigrationRoadmapInfo
{
    public IReadOnlyDictionary<string, MigrationRoadmapInfo> ModularMigrations { get; internal set; } = new Dictionary<string, MigrationRoadmapInfo>();
}
