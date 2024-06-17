using System.Diagnostics;
using System.Text;

namespace Sqlist.NET.Tools;
public class ProcessRunner : IProcessRunner
{
    public async Task<int> RunAsync(
        string executable,
        IReadOnlyList<string> args,
        string? workingDirectory = null,
        Action<string?>? handleOutput = null,
        Action<string?>? handleError = null,
        Action<string>? processCommandLine = null,
        CancellationToken cancellationToken = default)
    {
        var arguments = ToArguments(args);

        processCommandLine ??= Console.WriteLine;
        processCommandLine(executable + " " + arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = handleOutput is not null,
            RedirectStandardError = handleError is not null,
            CreateNoWindow = true
        };

        if (workingDirectory is not null)
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        using var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var tcs = new TaskCompletionSource<int>();

        void ProcessExited(object? sender, EventArgs e)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        process.Exited += ProcessExited;

        if (handleOutput is not null)
            process.OutputDataReceived += (sender, args) => handleOutput(args.Data);

        if (handleError is not null)
            process.ErrorDataReceived += (sender, args) => handleError(args.Data);

        try
        {
            process.Start();

            if (handleOutput is not null)
                process.BeginOutputReadLine();

            if (handleError is not null)
                process.BeginErrorReadLine();

            using (cancellationToken.Register(process.Kill))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error running process: {ex.Message}");
            return -1;
        }
        finally
        {
            process.Exited -= ProcessExited;
        }
    }

    public static string ToArguments(IReadOnlyList<string> args)
    {
        var builder = new StringBuilder();
        foreach (var arg in args)
        {
            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            if (string.IsNullOrEmpty(arg))
            {
                builder.Append("\"\"");
            }
            else if (!arg.Contains(' '))
            {
                builder.Append(arg);
            }
            else
            {
                builder.Append('"');
                builder.Append(arg.Replace("\\", "\\\\").Replace("\"", "\\\""));
                builder.Append('"');
            }
        }

        return builder.ToString();
    }
}
