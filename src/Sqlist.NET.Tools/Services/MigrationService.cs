using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Commands;

namespace Sqlist.NET.Tools.Services;

/// <summary>
///     Initializes a new instance of the <see cref="MigrationService"/> class.
/// </summary>
internal class MigrationService(MigrationCommand migrationCommand, IHostApplicationLifetime lifetime) : IHostedService
{
    static readonly string[] _cmdArgs = Environment.GetCommandLineArgs();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var app = new CommandLineApplication();

        migrationCommand.Configure(app);
        migrationCommand.OnCompleted += lifetime.StopApplication;

        await app.ExecuteAsync(_cmdArgs, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
