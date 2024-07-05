using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Migration;

namespace Sqlist.NET.Tools.Handlers;

/// <summary>
///     initializes a new instance of the <see cref="MigrationHandler"/> class.
/// </summary>
public class MigrationHandler(IServiceProvider services) : TransmittableCommandHandler
{
    public Version? FromVersion { get; set; }
    public Version? ToVersion { get; set; }

    public override async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        var migration = GetScopedServices();

        await migration.InitializeAsync(ToVersion, FromVersion);
        await migration.MigrateDataAsync();

        return 0;
    }

    private IMigrationContext GetScopedServices()
    {
        using var scope = services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMigrationContext>();
    }
}
