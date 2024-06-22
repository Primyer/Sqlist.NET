using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Properties;
using Sqlist.NET.Tools.Tests.TestUtilities;

namespace Sqlist.NET.Tools.Tests;
public class ConsoleServiceTests
{
    [Fact]
    public async Task StartAsync_ExecutesAndStopsApplication()
    {
        // Arrange
        var lifetimeMock = new Mock<IHostApplicationLifetime>();
        var executorMock = new Mock<IApplicationExecutor>();
        var execContextMock = new Mock<IExecutionContext>();
        var loggerMock = new LoggerMock<CommandHandlerService>();

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);

        var service = new CommandHandlerService(
            lifetimeMock.Object,
            executorMock.Object,
            execContextMock.Object,
            loggerMock
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
        var execContextMock = new Mock<IExecutionContext>();
        var loggerMock = new LoggerMock<CommandHandlerService>();

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);

        executorMock.Setup(e => e.ExecuteAsync(It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
                    .Returns(() => new CommandLineApplication().ExecuteAsync(["--test-option"], default));

        var service = new CommandHandlerService(
            lifetimeMock.Object,
            executorMock.Object,
            execContextMock.Object,
            loggerMock
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Simulate application start
        cts.Cancel();
        await Task.Delay(100); // Give some time for the task to run

        // Assert
        var logEntry = Assert.Single(loggerMock.LogEntries);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal(Resources.UnhandledException, logEntry.Message);
        Assert.IsType<UnrecognizedCommandParsingException>(logEntry.Exception);
        lifetimeMock.Verify(l => l.StopApplication(), Times.Once);
    }
}

internal class TestException(string message) : Exception(message);