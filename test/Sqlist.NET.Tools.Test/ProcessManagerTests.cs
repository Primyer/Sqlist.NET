using Microsoft.Extensions.Logging;
using Moq;

using System.Reflection;

namespace Sqlist.NET.Tools.Tests;
public class ProcessManagerTests
{
    private static readonly string[] stringArray = [
        "",
        "Good",
        "Good\\",
        "Needs quotes",
        "Needs escaping\\",
        "Needs escaping\\\\",
        "Needs \"escaping\"",
        "Needs \\\"escaping\"",
        "Needs escaping\\\\too"
    ];

    [Fact]
    public void ToArguments_Works()
    {
        // Arrange
        var expected = "\"\" "
            + "Good "
            + "Good\\ "
            + "\"Needs quotes\" "
            + "\"Needs escaping\\\\\" "
            + "\"Needs escaping\\\\\\\\\" "
            + "\"Needs \\\"escaping\\\"\" "
            + "\"Needs \\\\\\\"escaping\\\"\" "
            + "\"Needs escaping\\\\\\\\too\"";

        // Act
        var result = ToArguments(stringArray);

        // Assert
        Assert.Equal(expected, result);
    }

    private static string ToArguments(IReadOnlyList<string> args)
    {
        var type = typeof(ProcessManager);
        var func = nameof(ProcessManager.ToArguments);

        return (string)type.GetMethod(func, BindingFlags.Static | BindingFlags.Public)!.Invoke(null, [args])!;
    }

    [Fact]
    public async Task RunAsync_HandlesOutput()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProcessManager>>();
        var procRunner = new ProcessManager(mockLogger.Object);

        var output = string.Empty;
        var expected = "Hello, World!";

        // Act
        var process = procRunner.Prepare(
            "powershell.exe", ["-Command", "Write-Host 'Hello, World!'"],
            handleOutput: msg => output += msg);

        var exitCode = await process.RunAsync();

        // Assert
        Assert.Equal(0, exitCode);
        Assert.Equal(expected, output.Trim());
    }

    [Fact]
    public async Task RunAsync_HandlesErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProcessManager>>();
        var procRunner = new ProcessManager(mockLogger.Object);

        var errors = new List<string?>();

        // Act
        var process = procRunner.Prepare(
            "cmd.exe", ["/c", "non_existent_command"],
            handleError: errors.Add);

        var exitCode = await process.RunAsync();

        // Assert
        Assert.NotEqual(0, exitCode);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public async Task RunAsync_CancelsProcess()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ProcessManager>>();
        var procRunner = new ProcessManager(mockLogger.Object);

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        // Act
        var process = procRunner.Prepare(
            "powershell.exe", ["-Command", "Start-Sleep -Seconds 10"]);

        var runTask = process.RunAsync(cancellationToken);
        var delayTask = Task.Delay(100);

        await Task.WhenAny(delayTask, runTask); // Wait for either the delay or the process to complete

        if (delayTask.IsCompleted)
        {
            // Trigger cancellation if delayTask completes first
            cts.Cancel();
        }

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => runTask);
        Assert.True(process.Terminated, "Process should be terminated");
    }
}