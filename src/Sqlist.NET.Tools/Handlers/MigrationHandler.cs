using Sqlist.NET.Migration;
using Sqlist.NET.Tools.Logging;

namespace Sqlist.NET.Tools.Handlers;

/// <summary>
///     initializes a new instance of the <see cref="MigrationHandler"/> class.
/// </summary>
internal class MigrationHandler(MigrationContext service, IAuditor auditor) : TransmittableCommandHandler
{
    public override async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        var info = await service.InitializeAsync();

        auditor.WriteInformation(info.Title);
        auditor.WriteInformation($"Migrating database from version ({info.CurrentVersion}) to version ({info.TargetVersion})");
        auditor.WriteInformation(info.Description);
        auditor.WriteInformation("Migrating...");

        await service.MigrateDataAsync();
        return 0;
    }
}
