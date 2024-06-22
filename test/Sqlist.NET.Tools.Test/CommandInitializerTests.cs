using Moq;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Tests;
public class CommandInitializerTests
{
    [Fact]
    public async Task ExecuteAsync_CallsTransmitAsync_WhenHandlerIsTransmittable()
    {
        // Arrange
        var mockTransmitter = new Mock<ICommandTransmitter>();
        var mockExecContext = new Mock<IExecutionContext>();
        var initializer = new CommandInitializer(mockTransmitter.Object, mockExecContext.Object);
        var mockHandler = new Mock<TransmittableCommandHandler>();
        var cancellationToken = new CancellationToken();

        mockExecContext.Setup(c => c.IsTransmitter).Returns(true);

        // Act
        await initializer.ExecuteAsync(mockHandler.Object, cancellationToken);

        // Assert
        mockTransmitter.Verify(t => t.TransmitAsync(mockHandler.Object, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_CallsOnExecuteAsync_WhenHandlerIsNotTransmittable()
    {
        // Arrange
        var mockTransmitter = new Mock<ICommandTransmitter>();
        var mockExecContext = new Mock<IExecutionContext>();
        var initializer = new CommandInitializer(mockTransmitter.Object, mockExecContext.Object);
        var mockHandler = new Mock<ICommandHandler>();
        var cancellationToken = new CancellationToken();

        // Act
        await initializer.ExecuteAsync(mockHandler.Object, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.OnExecuteAsync(cancellationToken), Times.Once);
    }
}
