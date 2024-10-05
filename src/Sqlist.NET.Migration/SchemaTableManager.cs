using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Properties;

namespace Sqlist.NET.Migration;

internal class SchemaTableManager(IDbContext db, MigrationService migrationService, IOptions<MigrationOptions> options,
    ILogger<SchemaTableManager> logger) : ISchemaTableManager
{
    private readonly MigrationOptions _options = options.Value;
    private readonly DefinitionCollection _tableDefinition = new()
    {
        Columns =
        {
            Column(db, Consts.Id, column =>
            {
                column.Type = db.TypeMapper.TypeName<int>();
                column.IsSequence = true;
            }),
            Column(db, Consts.Package),
            Column(db, Consts.Version),
            Column(db, Consts.Parent, column => column.Type = db.TypeMapper.TypeName<int>()),
            Column(db, Consts.Title),
            Column(db, Consts.Description),
            Column(db, Consts.Summary),
            Column(db, Consts.Applied, column => column.Type = db.TypeMapper.TypeName<DateTime>())
        }
    };
    
    public async Task<Version?> GetSchemaVersionAsync(CancellationToken cancellationToken)
    {
        var exists = await migrationService.DoesSchemaTableExistAsync(cancellationToken);
        if (!exists)
        {
            logger.LogDebug("No schema table could be located; the database is presumed empty.");
            return null;
        }

        var mainPhase = await migrationService.GetLastSchemaPhaseAsync(cancellationToken);
        if (mainPhase is null) return null;

        return new(mainPhase.Version);
    }

    public async Task<MigrationOperationInfo> RetrieveSchemaDetailsAsync(CancellationToken cancellationToken)
    {
        var version = await GetSchemaVersionAsync(cancellationToken);
        
        return new MigrationOperationInfo()
        {
            CurrentVersion = version,
            ModularMigrations = await GetModularRoadmapInfo(version, cancellationToken)
        };
    }

    private async Task<IReadOnlyDictionary<string, MigrationRoadmapInfo>> GetModularRoadmapInfo(
        Version? version, CancellationToken cancellationToken)
    {
        if (version is null)
        {
            return new Dictionary<string, MigrationRoadmapInfo>();
        }

        var moduleInfo = new Dictionary<string, MigrationRoadmapInfo>();
        var modularPhases = await migrationService.GetModularSchemaPhasesAsync(cancellationToken);

        foreach (var phase in modularPhases)
        {
            if (phase.Package is null) continue;
            moduleInfo.Add(phase.Package, new MigrationRoadmapInfo { CurrentVersion = new Version(phase.Version) });
        }

        return moduleInfo;
    }

    public MigrationPhase GetSchemaTableDefinition()
    {
        var table = _options.SchemaTable ?? Consts.DefaultSchemaTable;
        var phase = new MigrationPhase();
        
        phase.Guidelines.Create.Add(table, _tableDefinition);
        return phase;
    }
    
    private static KeyValuePair<string, ColumnDefinition> Column(IDbContext db, string name,
        Action<ColumnDefinition>? configure = null)
    {
        var definition = new ColumnDefinition();
        
        configure?.Invoke(definition);
        definition.Type ??= db.TypeMapper.TypeName<string>();
        
        return KeyValuePair.Create(name, definition);
    }
}