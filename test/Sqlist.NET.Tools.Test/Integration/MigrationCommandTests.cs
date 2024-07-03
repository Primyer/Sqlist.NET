using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Tests.TestUtilities;

using Xunit.Abstractions;

namespace Sqlist.NET.Tools.Tests.Integration;
public class MigrationCommandTests(ITestOutputHelper output)
{
    [Fact]
    public async Task MigrationCommand_SuccessfullyMigratesDatabase()
    {
        // Arrange
        var logs = new List<string?>();

        var mockContext = new Mock<IExecutionContext>();
        var auditorMock = new Mock<Auditor>(mockContext.Object);

        auditorMock.Setup(a => a.WriteLine(It.IsAny<string?>())).Callback((string? message) => logs.Add(message));

        var host = new HostBuilderMock()
            .UseCommandLineApplication()
            .ConfigureServices(services =>
            {
                services.Remove(ServiceDescriptor.Singleton<IAuditor, Auditor>());
                services.AddSingleton<IAuditor>(auditorMock.Object);
            })
            .Build();

        var path = ProjectHelpers.GetSandboxProjectPath();
        var args = new[] { "migrate", "--project", path, "--no-color", "--verbose", "--prefix-output" };

        var executor = host.Services.GetRequiredService<IApplicationExecutor>();

        // Act
        var exitCode = await executor.ExecuteAsync(args, CancellationToken.None);

        foreach (var log in logs)
            output.WriteLine(log ?? "");
    }
}