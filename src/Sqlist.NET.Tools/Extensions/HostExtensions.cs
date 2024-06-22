using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Extensions;
internal static class HostExtensions
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
}
