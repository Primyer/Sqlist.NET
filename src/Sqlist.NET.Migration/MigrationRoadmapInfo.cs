using System;

using Microsoft.Extensions.Logging;

namespace Sqlist.NET.Migration;
public class MigrationRoadmapInfo
{
    private const string LinePrefix = "==> ";
    
    public Version? CurrentVersion { get; internal set; }

    public Version TargetVersion { get; set; } = new();

    public Version LatestVersion { get; internal set; } = new();

    public string Title { get; internal set; } = null!;

    public string? Description { get; internal set; }

    public string? SchemaChanges { get; internal set; }

    internal void Log(ILogger logger)
    {
        logger.LogInformation("{linePrefix} Current version = {version}", LinePrefix, CurrentVersion);
        logger.LogInformation("{linePrefix} Migration to version = {version}", LinePrefix, LatestVersion);
        logger.LogInformation("{linePrefix} Title = {title}", LinePrefix, Title);
        logger.LogInformation("{linePrefix} Description = {description}", LinePrefix, Description);
        logger.LogDebug("{linePrefix} Schema changes = {schema}", LinePrefix, SchemaChanges);
    }
}
