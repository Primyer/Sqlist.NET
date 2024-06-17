using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Services;

namespace Sqlist.NET.Tools.Extensions;
internal static class HostExtensions
{
    public static IHostBuilder UseCommandLineApplication<TCommand>(this IHostBuilder host) where TCommand : ICommand
    {
        var app = new CommandLineApplication { Name = "dotnet sqlist" };

        host.ConfigureServices(services =>
        {
            services.AddCommonServices();
            services.TryAddSingleton(app);

            services.AddHostedService<ConsoleService<TCommand>>();
        });

        return host;
    }
}
