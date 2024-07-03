using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;
using Sqlist.NET.Tools.Tests.TestUtilities;

namespace Sqlist.NET.Tools.Tests;
public class CommandHandlerServiceTests
{
    [Fact]
    public async Task StartAsync_ExecutesAndStopsApplication()
    {
        // Arrange
        var lifetimeMock = new Mock<IHostApplicationLifetime>();
        var executorMock = new Mock<IApplicationExecutor>();
        var auditorMock = new Mock<IAuditor>();

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);

        var service = new CommandHandlerService(
            lifetimeMock.Object,
            executorMock.Object,
            auditorMock.Object
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Simulate application start
        cts.Cancel();
        await Task.Delay(100); // Give some time for the task to run

        // Assert
        executorMock.Verify(c => c.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()), Times.Once);
        lifetimeMock.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_LogsExceptionAndStopsApplication()
    {
        // Arrange
        var lifetimeMock = new Mock<IHostApplicationLifetime>();
        var executorMock = new Mock<IApplicationExecutor>();
        var auditorMock = new Mock<IAuditor>();

        List<LogEntry> logEntries = [];
        
        auditorMock.Setup(a => a.WriteError(It.IsAny<Exception>(), It.IsAny<string?>()))
            .Callback((Exception ex, string? message) =>
            {
                logEntries.Add(new()
                {
                    Exception = ex,
                    Message = message
                });
            });

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);

        executorMock.Setup(e => e.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                    .Returns(() => new CommandLineApplication().ExecuteAsync(["--test-option"], default));

        var service = new CommandHandlerService(
            lifetimeMock.Object,
            executorMock.Object,
            auditorMock.Object
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Simulate application start
        cts.Cancel();
        await Task.Delay(100); // Give some time for the task to run

        // Assert
        var logEntry = Assert.Single(logEntries);
        Assert.Equal(Resources.UnhandledException, logEntry.Message);
        Assert.IsType<UnrecognizedCommandParsingException>(logEntry.Exception);
        lifetimeMock.Verify(l => l.StopApplication(), Times.Once);
    }
}

internal class TestException(string message) : Exception(message);