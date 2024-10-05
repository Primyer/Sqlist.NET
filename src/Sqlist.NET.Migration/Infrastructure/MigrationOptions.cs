using System.Collections.Generic;
using System.Reflection;

using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration.Infrastructure;
public class MigrationOptions : MigrationAssetInfo
{
    private Assembly? _scriptsAssembly;
    private Assembly? _roadmapAssembly;

    public override Assembly? ScriptsAssembly
    {
        get => _scriptsAssembly ??= Assembly.GetEntryAssembly();
        set => _scriptsAssembly = value;
    }

    public override Assembly? RoadmapAssembly
    {
        get => _roadmapAssembly ??= Assembly.GetEntryAssembly();
        set => _roadmapAssembly = value;
    }

    public string SchemaTable { get; set; } = Consts.DefaultSchemaTable;
    public string? SchemaTableSchema { get; set; }

    public Dictionary<string, MigrationAssetInfo> ModularAssets { get; } = [];
}
