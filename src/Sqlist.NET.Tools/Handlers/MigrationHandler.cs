using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Migration;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Handlers;

/// <summary>
///     initializes a new instance of the <see cref="MigrationHandler"/> class.
/// </summary>
internal class MigrationHandler(IServiceProvider services) : TransmittableCommandHandler
{
    public Version? FromVersion { get; set; }
    public Version? ToVersion { get; set; }

    public override async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        var migration = services.GetRequiredService<IMigrationContext>();
        var auditor = services.GetRequiredService<IAuditor>();

        var info = await migration.InitializeAsync(ToVersion, FromVersion);
        LogInfo(auditor, info);

        await migration.MigrateDataAsync();
        return 0;
    }

    private static void LogInfo(IAuditor auditor, MigrationOperationInformation info)
    {
        auditor.WriteInformation(info.Title);

        var message = info.CurrentVersion is not null
            ? string.Format(Resources.MigratingFromVersion, info.CurrentVersion, info.TargetVersion)
            : string.Format(Resources.MigratingToVersion, info.TargetVersion);

        auditor.WriteInformation(message);
        auditor.WriteInformation(info.Description);
        auditor.WriteInformation("Migrating...");
    }
}
