using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools;

/// <summary>
///     Initializes a new instance of the <see cref="CommandHandlerService"/> class.
/// </summary>
internal class CommandHandlerService(IHostApplicationLifetime lifetime, IApplicationExecutor application, IExecutionContext context, IAuditor auditor) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await application.ExecuteAsync(context.CommandLineArgs, cancellationToken);
                }
                catch (Exception ex)
                {
                    auditor.WriteError(ex, Resources.UnhandledException);
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
