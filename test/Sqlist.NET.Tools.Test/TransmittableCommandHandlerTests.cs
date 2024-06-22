using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Tests;
public class TransmittableCommandHandlerTests
{
    [Fact]
    public void Initialize_SetsCommandOptions()
    {
        // Arrange
        var command = new CommandLineApplication();
        var handler = new TestTransmittableCommandHandler();

        // Act
        handler.Initialize(command);

        // Assert
        Assert.NotNull(handler.Project);
        Assert.NotNull(handler.Framework);
        Assert.NotNull(handler.Configuration);
        Assert.NotNull(handler.Runtime);
        Assert.NotNull(handler.LaunchProfile);
        Assert.NotNull(handler.Force);
        Assert.NotNull(handler.NoBuild);
        Assert.NotNull(handler.NoRestore);
    }
}