using McMaster.Extensions.CommandLineUtils;

using Moq;

using Sqlist.NET.Tools.Handlers;
using System.Reflection;

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

    [Fact]
    public void GetOptions_ReturnsAllProperties()
    {
        // Arrange
        var handler = new Mock<TransmittableCommandHandler> { CallBase = true }.Object;

        // Get all public instance properties of type CommandOption? from the handler
        var properties = typeof(TransmittableCommandHandler)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(p => p.PropertyType == typeof(CommandOption))
            .ToArray();

        // Act
        var options = handler.GetOptions();

        // Assert
        Assert.Equal(properties.Length, options.Length);

        foreach (var property in properties)
        {
            // Check if the property value exists in the options array
            var value = property.GetValue(handler);
            Assert.Contains(value, options);
        }
    }
}