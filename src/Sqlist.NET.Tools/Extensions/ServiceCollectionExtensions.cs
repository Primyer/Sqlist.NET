using Microsoft.Extensions.DependencyInjection.Extensions;

using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Tools;
using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Infrastructure;

using ExecutionContext = Sqlist.NET.Tools.Infrastructure.ExecutionContext;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static SqlistBuilder AddSqlistTools(this SqlistBuilder builder)
    {
        builder.Services.AddCommonServices();
        builder.Services.TryAddSingleton<IApplicationExecutor, ToolCliExecutor>();

        return builder;
    }

    internal static void AddCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<MigrationCommand>();

        services.TryAddSingleton<IProcessManager, ProcessManager>();
        services.TryAddSingleton<ICommandTransmitter, CommandTransmitter>();
        services.TryAddSingleton<ICommandInitializer, CommandInitializer>();
        services.TryAddSingleton<IExecutionContext, ExecutionContext>();

        services.AddHostedService<CommandHandlerService>();
    }
}
