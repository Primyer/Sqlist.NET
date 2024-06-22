using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Moq;

using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Infrastructure;
using Sqlist.NET.Tools.Services;
using Sqlist.NET.Tools.Tests.TestUtilities;

namespace Sqlist.NET.Tools.Tests;
public class ConsoleServiceTests
{
    [Fact]
    public async Task StartAsync_ExecutesRootCommandAndStopsApplication()
    {
        // Arrange
        var lifetimeMock = new Mock<IHostApplicationLifetime>();
        var commandMock = new Mock<ICommand>();
        var execContextMock = new Mock<IExecutionContext>();
        var loggerMock = new LoggerMock<ConsoleService<ICommand>>();

        var app = new CommandLineApplication();

        // Setup command
        commandMock.Setup(c => c.Configure(It.IsAny<CommandLineApplication>())).Callback<CommandLineApplication>(app => app.OnExecute(() => 0));

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);

        var service = new ConsoleService<ICommand>(
            lifetimeMock.Object,
            app,
            commandMock.Object,
            execContextMock.Object,
            loggerMock
        );

        // Act
        await service.StartAsync(CancellationToken.None);

        // Simulate application start
        cts.Cancel();
        await Task.Delay(100); // Give some time for the task to run

        // Assert
        commandMock.Verify(c => c.Configure(app), Times.Once);
        lifetimeMock.Verify(l => l.StopApplication(), Times.Once);
    }

    [Fact]
    public async Task StartAsync_LogsExceptionAndStopsApplication()
    {
        // Arrange
        var lifetimeMock = new Mock<IHostApplicationLifetime>();
        var commandMock = new Mock<ICommand>();
        var execContextMock = new Mock<IExecutionContext>();
        var loggerMock = new LoggerMock<ConsoleService<ICommand>>();

        var app = new CommandLineApplication();

        // Setup lifetime cancellation tokens
        var cts = new CancellationTokenSource();
        lifetimeMock.Setup(l => l.ApplicationStarted).Returns(cts.Token);
        execContextMock.Setup(c => c.CommandLineArgs).Returns(["--test-option"]);

        var service = new ConsoleService<ICommand>(
            lifetimeMock.Object,
            app,
            commandMock.Object,
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
        Assert.Equal("Unhandled exception!", logEntry.Message);
        Assert.IsType<UnrecognizedCommandParsingException>(logEntry.Exception);
        lifetimeMock.Verify(l => l.StopApplication(), Times.Once);
    }
}

internal class TestException(string message) : Exception(message);