using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Extensions;
public static class HostExtensions
{
    public static IHostBuilder UseCommandLineApplication(this IHostBuilder host)
    {
        host.ConfigureServices(services =>
        {
            services.AddCommonServices();
            services.TryAddSingleton<IApplicationExecutor, ToolCliExecutor>();
        });

        return host;
    }

    public static TBuilder UseSqlistTools<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var args = string.Join(' ', Environment.GetCommandLineArgs());
        if (args.StartsWith(Resources.RootCommandName))
        {
            builder.Logging.ClearProviders();
            builder.Services.RemoveServices<IHostedService>();

            builder.Services.AddCommonServices();
            builder.Services.TryAddSingleton<IApplicationExecutor, ToolCliExecutor>();
        }

        return builder;
    }
}
