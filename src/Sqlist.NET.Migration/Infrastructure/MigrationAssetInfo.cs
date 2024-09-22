using System.Reflection;

namespace Sqlist.NET.Migration.Infrastructure;
public class MigrationAssetInfo
{
    public virtual Assembly? ScriptsAssembly { get; set; }
    public virtual Assembly? RoadmapAssembly { get; set; }

    public string ScriptsPath { get; set; } = "Migration.Scripts";
    public string RoadmapPath { get; set; } = "Migration.Roadmap";
}
