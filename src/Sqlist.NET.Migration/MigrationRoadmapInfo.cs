using System;

using Microsoft.Extensions.Logging;

using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration;
public class MigrationRoadmapInfo
{
    private const string LinePrefix = "==> ";
    
    public Version? CurrentVersion { get; internal init; }
    public Version TargetVersion { get; private set; } = new();
    public Version LatestVersion { get; private set; } = new();
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? SchemaChanges { get; private set; }

    internal void SetFromPhase(MigrationPhase phase, DataTransactionMap datamap, Version? targetVersion)
    {
        Title = phase.Title;
        Description = phase.Description;
        LatestVersion = phase.Version;
        SchemaChanges = datamap.GenerateSummary();
        TargetVersion = targetVersion is not null && phase.Version != targetVersion
            ? targetVersion : phase.Version;
    }

    internal void Log(ILogger logger)
    {
        logger.LogInformation("{linePrefix} Current version = {version}", LinePrefix, CurrentVersion);
        logger.LogInformation("{linePrefix} Migration to version = {version}", LinePrefix, LatestVersion);
        logger.LogInformation("{linePrefix} Title = {title}", LinePrefix, Title);
        logger.LogInformation("{linePrefix} Description = {description}", LinePrefix, Description);
        logger.LogDebug("{linePrefix} Schema changes = {schema}", LinePrefix, SchemaChanges);
    }
}
