using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Cli.Extensions;
internal static class ServiceCollectionExtensions
{
    public static void AddCliServices(this IServiceCollection services)
    {
        services.AddCommonServices();

        services.TryAddSingleton<IProcessManager, ProcessManager>();
        services.TryAddSingleton<ICommandTransmitter, CommandTransmitter>();
        services.TryAddSingleton<ICommandInitializer, CliCommandInitializer>();
        services.TryAddSingleton<IApplicationExecutor, ToolCliExecutor>();
    }
}
