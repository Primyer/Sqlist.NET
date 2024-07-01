using System;

namespace Sqlist.NET.Migration.Deserialization
{
    public class MigrationOperationInformation
    {
        public Version? CurrentVersion { get; internal set; }

        public Version TargetVersion { get; set; } = new();

        public Version LatestVersion { get; internal set; } = new();

        public string? Title { get; internal set; }

        public string? Description { get; internal set; }

        public string? SchemaChanges { get; internal set; }
    }
}
