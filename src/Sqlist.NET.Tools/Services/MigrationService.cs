using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Services;

/// <summary>
///     Initializes a new instance of the <see cref="MigrationService"/> class.
/// </summary>
internal class MigrationService(MigrationCommand migrationCommand, IHostApplicationLifetime lifetime, IExecutionContext context) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var args = context.CommandLineArgs;

        lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    var app = new CommandLineApplication();

                    migrationCommand.Configure(app);
                    migrationCommand.OnCompleted += lifetime.StopApplication;

                    await app.ExecuteAsync(args, cancellationToken);
                }
                finally
                {
                    lifetime.StopApplication();
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
