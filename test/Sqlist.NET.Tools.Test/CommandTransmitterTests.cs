using McMaster.Extensions.CommandLineUtils;

using Moq;

using Sqlist.NET.Tools.Cli;
using Sqlist.NET.Tools.Exceptions;
using Sqlist.NET.Tools.Extensions;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools.Tests;
public class CommandTransmitterTests
{
    [Fact]
    public void AddArguments_AddsArgumentsCorrectly()
    {
        // Arrange
        var args = new List<string>();
        var app = new CommandLineApplication();
        var projectOption = app.Option("-p|--project <PROJECT>", "The project file", CommandOptionType.SingleValue);

        // Simulate setting the value of the project option
        app.Parse("-p MyProject.csproj");

        // Act
        CommandTransmitter.AddArguments(args, [projectOption]);

        // Assert
        Assert.Contains("--project", args);
        Assert.Contains("MyProject.csproj", args);
    }

    [Fact]
    public void AddArguments_HandlesShortNameCorrectly()
    {
        // Arrange
        var args = new List<string>();
        var app = new CommandLineApplication();
        var shortOption = app.Option("-s <VALUE>", "A short option", CommandOptionType.SingleValue);

        // Simulate setting the value of the short option
        app.Parse("-s Value");

        // Act
        CommandTransmitter.AddArguments(args, [shortOption]);

        // Assert
        Assert.Contains("-s", args);
        Assert.Contains("Value", args);
    }

    [Fact]
    public void AddArguments_HandlesNoValueOptionCorrectly()
    {
        // Arrange
        var args = new List<string>();
        var app = new CommandLineApplication();
        var noValueOption = app.Option("-n|--novalue", "An option with no value", CommandOptionType.NoValue);

        // Simulate setting the no-value option
        app.Parse("--novalue");

        // Act
        CommandTransmitter.AddArguments(args, [noValueOption]);

        // Assert
        Assert.Contains("--novalue", args);
        Assert.DoesNotContain("-n", args);  // Should only contain the long name since it's set to --novalue
    }

    [Fact]
    public void GetOptionName_ReturnsCorrectName()
    {
        // Arrange
        var app = new CommandLineApplication();
        var longOption = app.Option("--long <VALUE>", "A long option", CommandOptionType.SingleValue);
        var shortOption = app.Option("-s <VALUE>", "A short option", CommandOptionType.SingleValue);
        var symbolOption = app.Option("-# <VALUE>", "A symbol option", CommandOptionType.SingleValue);

        // Act & Assert
        Assert.Equal("--long", longOption.GetOptionName());
        Assert.Equal("-s", shortOption.GetOptionName());
        Assert.Equal("-#", symbolOption.GetOptionName());
    }

    [Fact]
    public async Task TransmitAsync_ThrowsException_WhenProjectIsNull()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessManager>();
        var mockContext = new Mock<IExecutionContext>();

        var transmitter = new CommandTransmitter(mockProcessRunner.Object, mockContext.Object);
        var handler = new TestTransmittableCommandHandler();

        // Act & Assert
        await Assert.ThrowsAsync<CommandTransmissionException>(() =>
            transmitter.TransmitAsync(handler, CancellationToken.None));
    }

    [Fact]
    public async Task TransmitAsync_ThrowsException_WhenProjectHasNoValue()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessManager>();
        var mockContext = new Mock<IExecutionContext>();

        var transmitter = new CommandTransmitter(mockProcessRunner.Object, mockContext.Object);
        var handler = new TestTransmittableCommandHandler();
        var app = new CommandLineApplication();

        handler.Initialize(app);

        // Simulate setting the value of the project option to an empty string
        app.Parse("-p ");

        // Act & Assert
        await Assert.ThrowsAsync<CommandTransmissionException>(() =>
            transmitter.TransmitAsync(handler, CancellationToken.None));
    }

    [Fact]
    public async Task TransmitAsync_RunsProcess_WhenProjectIsValid()
    {
        // Arrange
        var mockProcessRunner = new Mock<IProcessManager>();
        var mockContext = new Mock<IExecutionContext>();
        var mockProcess = new Mock<IProcess>();
        var app = new CommandLineApplication();

        mockContext.Setup(c => c.Application).Returns(app);
        mockContext.Setup(c => c.SelectedCommand).Returns(app);

        mockProcessRunner.Setup(pr => pr.Prepare(
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<string>(),
            It.IsAny<Action<string?>>(),
            It.IsAny<Action<string?>>(),
            It.IsAny<Action<string?>>()))
            .Returns(mockProcess.Object);

        mockProcess.Setup(p => p.RunAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var transmitter = new CommandTransmitter(mockProcessRunner.Object, mockContext.Object);
        var handler = new TestTransmittableCommandHandler();
        
        handler.Initialize(app);

        // Simulate setting the value of the project option
        app.Parse("-p MyProject.csproj");

        // Act
        await transmitter.TransmitAsync(handler, CancellationToken.None);

        // Assert
        mockProcessRunner.Verify(pr => pr.Prepare(
            It.Is<string>(s => s == "dotnet"),
            It.Is<IReadOnlyList<string>>(a => a.Contains("run") && a.Contains("MyProject.csproj")),
            It.IsAny<string>(),
            It.IsAny<Action<string?>>(),
            It.IsAny<Action<string?>>(),
            It.IsAny<Action<string?>>()), Times.Once);

        mockProcess.Verify(p => p.RunAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void AddTransmittableArgs_ExcludesHandlerOptions_AddsRemainingOptions()
    {
        // Arrange
        var args = new List<string>();

        var handler = new TestTransmittableCommandHandler();
        var command = new CommandLineApplication();

        var expectedArgs = new List<string> { "subcommand", "--other", "the other", "--test", "the test" };

        var selectedCommand = command.Command("subcommand", cmd =>
        {
            cmd.Option("-o|--other <OTHER>", "Other option", CommandOptionType.SingleValue);
            handler.Configure(cmd);
        });

        // Act
        command.Parse([.. expectedArgs, "--project", "path/to/project", "--framework", ".net8.0", "-c", "Debug"]);
        CommandTransmitter.AddTransmittableArgs(args, handler, selectedCommand);

        // Assert
        Assert.Equal(expectedArgs, args);
    }
}