using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;
using Sqlist.NET.Tools.Tests.TestUtilities;

using Xunit.Abstractions;

namespace Sqlist.NET.Tools.Tests.Integration;
public class CommandTransmissionTests(ITestOutputHelper output)
{
    [Fact]
    public async Task SqlistNetTools_CommandTransmission_WorksCorrectly()
    {
        // Arrange
        List<string?> logEntries = [];

        var auditorMock = new Mock<Auditor>();
        auditorMock.Setup(a => a.WriteLine(It.IsAny<string?>())).Callback((string? message) => logEntries.Add(message));

        var host = new HostBuilderMock()
            .UseCommandLineApplication()
            .ConfigureServices(services =>
            {
                services.Remove(ServiceDescriptor.Singleton<IAuditor, Auditor>());
                services.AddSingleton<IAuditor>(auditorMock.Object);
            })
            .Build();

        var executor = host.Services.GetRequiredService<IApplicationExecutor>();
        var sandboxPath = ProjectHelpers.GetSandboxProjectPath();

        // Act
        await executor.ExecuteAsync(["test", "--project", sandboxPath, "--no-color", "--verbose"], CancellationToken.None);

        foreach (var log in logEntries)
            output.WriteLine(log ?? "");

        // Assert
        var entry = Assert.Single(logEntries);
        Assert.Equal(Resources.CommandSucceeded, entry);
    }
}
