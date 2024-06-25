﻿using Microsoft.Extensions.DependencyInjection.Extensions;

using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Tools;
using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;

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
        services.TryAddSingleton<IAuditor, Auditor>();
#if DEBUG
        services.TryAddSingleton<TestHandler>();
        services.TryAddSingleton<TestCommand>();
#endif
        services.TryAddSingleton<MigrationHandler>();
        services.TryAddSingleton<MigrationCommand>();

        services.TryAddSingleton<IProcessManager, ProcessManager>();
        services.TryAddSingleton<ICommandTransmitter, CommandTransmitter>();
        services.TryAddSingleton<ICommandInitializer, CommandInitializer>();
        services.TryAddSingleton<IExecutionContext, ExecutionContext>();

        services.AddHostedService<CommandHandlerService>();
    }
}
