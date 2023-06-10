using System.Reflection;

namespace Sqlist.NET.Migration.Infrastructure
{
    public class MigrationOptions
    {
        public string? SchemaTable { get; set; } = "schema_phases";
        public string? SchemaTableSchema { get; set; }

        public Assembly? ScriptsAssembly { get; set; }
        public Assembly? RoadmapAssembly { get; set; }

        public string ScriptsPath { get; set; } = "Scripts";
        public string RoadmapPath { get; set; } = "Roadmap";
    }
}
