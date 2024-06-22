using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Services;

/// <summary>
///     Initializes a new instance of the <see cref="ConsoleService"/> class.
/// </summary>
internal class ConsoleService<TCommand>(
    IHostApplicationLifetime lifetime,
    CommandLineApplication app,
    TCommand rootCommand,
    IExecutionContext context,
    ILogger<ConsoleService<TCommand>> logger) : IHostedService where TCommand : ICommand
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        context.IsTransmitter = true;
        var args = context.CommandLineArgs;

        lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    rootCommand.Configure(app);
                    await app.ExecuteAsync(args, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unhandled exception!");
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
