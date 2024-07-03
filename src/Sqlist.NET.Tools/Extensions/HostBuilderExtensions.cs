using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;
using Sqlist.NET.Tools.Utilities;

namespace Sqlist.NET.Tools.Extensions;
public static class HostExtensions
{
    public static TBuilder UseSqlistTools<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (CommandLine.String.StartsWith(Resources.RootCommandName))
        {
            builder.Logging.ClearProviders();

            builder.Services.RemoveAll<IConfigureOptions<LoggerFilterOptions>>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AuditorLoggerProvider>());

            builder.Logging.SetMinimumLevel(LogLevel.None);
            builder.Logging.AddFilter("Sqlist.NET", LogLevel.Trace);

            builder.Services.RemoveServices<IHostedService>();
            builder.Services.AddSqlistTools();
        }
#if TEST
        else
        {
            throw new InvalidOperationException(string.Format(Resources.RootCommandExpectedException, Resources.RootCommandName));
        }
#endif

        return builder;
    }
}
