using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Sqlist.NET.Extensions;
using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Tests.TestUtilities;

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
        services.AddSqlist().AddSqlistTools();

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
    public async Task DotNetTool_DependencyInjection_WorksCorrectly()
    {
        // Arrange
        var host = new HostBuilderMock()
            .ConfigureServices(services => services.AddLogging())
            .UseCommandLineApplication()
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
}
