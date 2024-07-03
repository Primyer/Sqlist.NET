using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

namespace Sqlist.NET.Tools.Tests.TestUtilities;
internal class HostBuilderMock : IHostApplicationBuilder, IHostBuilder
{
    private readonly ServiceCollection _services = new();
    private ILoggingBuilder? _logging;

    public IDictionary<object, object> Properties => throw new NotImplementedException();

    public IConfigurationManager Configuration => throw new NotImplementedException();

    public IHostEnvironment Environment => throw new NotImplementedException();

    public ILoggingBuilder Logging => _logging!;

    public IMetricsBuilder Metrics => throw new NotImplementedException();

    public IServiceCollection Services => throw new NotImplementedException();

    public IHost Build()
    {
        var hostMock = new Mock<IHost>();
        var appLifetimeMock = new Mock<IHostApplicationLifetime>();

        _services.AddSingleton(appLifetimeMock.Object);

        var serviceProvider = _services.BuildServiceProvider();

        hostMock.Setup(h => h.Services).Returns(serviceProvider);
        hostMock.Setup(h => h.StartAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken cancellationToken) =>
            {
                var services = serviceProvider.GetServices<IHostedService>();

                foreach (var service in services)
                {
                    await service.StartAsync(cancellationToken);
                }
            });

        return hostMock.Object;
    }

    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        throw new NotImplementedException();
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        throw new NotImplementedException();
    }

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }

    public IHostBuilder ConfigureLogging(Action<ILoggingBuilder> configureDelegate)
    {
        _services.AddLogging(logging =>
        {
            configureDelegate(logging);
            _logging = logging;
        });

        return this;
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        throw new NotImplementedException();
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        configureDelegate(null!, _services);
        return this;
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }
}
