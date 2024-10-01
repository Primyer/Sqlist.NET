using McMaster.Extensions.CommandLineUtils;

using Moq;

using Sqlist.NET.Tools.Commands;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Logging;

namespace Sqlist.NET.Tools.Tests;
public class CommandsTests
{
    private class TestHandledCommand(
        ICommandHandler handler, ICommandInitializer initializer)
        : CommandBase<ICommandHandler>(handler, initializer)
    {
    }

    [Fact]
    public async Task CommandBase_ExecuteAsync_ValidatesAndExecutesHandler()
    {
        // Arrange
        var mockInitializer = new Mock<ICommandInitializer>();
        var handler = new Mock<ICommandHandler>();
        var command = new TestHandledCommand(handler.Object, mockInitializer.Object);

        var cancellationToken = new CancellationToken(); // Mock CancellationToken if needed

        // Act
        await command.ExecuteAsync(cancellationToken);

        // Assert
        mockInitializer.Verify(i => i.ExecuteAsync(handler.Object, cancellationToken), Times.Once);
        // Add more assertions based on Validate() method and other behavior
    }
    
    private class TestCommandHandler : TransmittableCommandHandler
    {
        public override Task<int> OnExecuteAsync(CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private class TestCommand(TestCommandHandler handler, ICommandInitializer initializer) : CommandBase<TestCommandHandler>(handler, initializer)
    {
        public bool ValidateCalled { get; private set; }

        protected override void Validate()
        {
            ValidateCalled = true;
        }
    }

    [Fact]
    public async Task ExecuteAsync_SetsReporterProperties_AndCallsExecuteAsyncOnInitializer()
    {
        // Arrange
        var handlerMock = new Mock<TestCommandHandler>();
        var initializerMock = new Mock<ICommandInitializer>();
        var command = new TestCommand(handlerMock.Object, initializerMock.Object);
        var app = new CommandLineApplication();

        // Act
        command.Configure(app);
        await app.ExecuteAsync(["--verbose", "--no-color", "--prefix-output"], new());

        // Assert
        Assert.True(Auditor.IsVerbose);
        Assert.True(Auditor.NoColor);
        Assert.True(Auditor.PrefixOutput);

        initializerMock.Verify(i => i.ExecuteAsync(handlerMock.Object, It.IsAny<CancellationToken>()), Times.Once);
        Assert.True(command.ValidateCalled);
    }
}
