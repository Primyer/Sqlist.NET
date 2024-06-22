﻿using System.Diagnostics;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Sqlist.NET.Tools;
internal class ProcessManager(ILogger<ProcessManager> logger) : IProcessManager
{
    public IProcess Prepare(
        string executable,
        IReadOnlyList<string> args,
        string? workingDirectory = null,
        Action<string?>? handleOutput = null,
        Action<string?>? handleError = null,
        Action<string>? processCommandLine = null)
    {
        var arguments = ToArguments(args);

        processCommandLine ??= message => logger.LogInformation("{message}", message);
        processCommandLine($"{executable} {arguments}");

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = handleOutput is not null,
            RedirectStandardError = handleError is not null,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? string.Empty
        };

        var process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };

        var internalProcess = new ManagedProcess(process, logger);

        if (handleOutput is not null)
            internalProcess.OutputDataReceived += handleOutput;

        if (handleError is not null)
            internalProcess.ErrorDataReceived += handleError;

        return internalProcess;
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