using Microsoft.Extensions.DependencyInjection.Extensions;

using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Tools;
using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static SqlistBuilder AddSqlistTools(this SqlistBuilder builder)
    {
        builder.Services.TryAddSingleton<MigrationCommand>();
        builder.Services.AddHostedService<MigrationService>();
        builder.Services.AddCommonServices();

        return builder;
    }

    internal static void AddCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ICommandInitializer, CommandInitializer>();
        services.TryAddSingleton<ICommandTransmitter, CommandTransmitter>();
        services.TryAddSingleton<IProcessRunner, ProcessRunner>();
    }
}
