extern alias dotnet_sqlist;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

using Sqlist.NET.Extensions;
using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Tests.TestUtilities;
using Sqlist.NET.Tools.Utilities;

namespace Sqlist.NET.Tools.Tests.Integration;
public class DependencyRegistrationTests
{
    [Fact]
    public async Task TargetSide_DependencyInjection_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        var appLifetimeMock = new Mock<IHostApplicationLifetime>();

        services.AddLogging();
        services.AddSingleton(appLifetimeMock.Object);
        services.AddSqlist();
        services.AddSqlistTools();

        var serviceProvider = services.BuildServiceProvider();
        var hostedServices = serviceProvider.GetServices<IHostedService>();

        // Assert
        try
        {
            foreach (var hostedService in hostedServices)
            {
                await hostedService.StartAsync(CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but got: {ex}");
        }
    }

    [Fact]
    public void UseSqlistTools_RemovesHostedServicesAndAddsSqlistTools_WhenCommandStartsWithRootCommandName()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddHostedService<MockHostedService>();

        var loggingBuilderMock = new Mock<ILoggingBuilder>();

        loggingBuilderMock.Setup(lb => lb.Services).Returns(services);
        loggingBuilderMock.Object.AddConsole();

        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.Setup(b => b.Services).Returns(services);
        builderMock.Setup(b => b.Logging).Returns(loggingBuilderMock.Object);

        CommandLine.Args = [dotnet_sqlist::Sqlist.NET.Tools.Properties.Resources.RootCommandName, "arg1", "arg2"];

        // Act
        builderMock.Object.UseSqlistTools();

        // Assert
        var loggerProvider = Assert.Single(services, s => s.ServiceType == typeof(ILoggerProvider));
        
        Assert.Equal(typeof(AuditorLoggerProvider), loggerProvider.ImplementationType);
        Assert.DoesNotContain(services, s => s.ServiceType == typeof(MockHostedService));
        Assert.Contains(services, s => s.ImplementationType == typeof(CommandHandlerService));
    }

    [Fact]
    public async Task DotNetTool_DependencyInjection_WorksCorrectly()
    {
        // Arrange
        var host = new HostBuilderMock()
            .ConfigureServices(services =>
            {
                services.AddLogging();
                services.AddCliServices();
            })
            .Build();

        // Assert
        try
        {
            await host.StartAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Expected no exception, but got: {ex}");
        }
    }

    public class MockHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
