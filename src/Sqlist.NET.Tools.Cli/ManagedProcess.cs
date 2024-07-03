using Sqlist.NET.Tools.Logging;

using System.Diagnostics;

namespace Sqlist.NET.Tools.Cli;
internal class ManagedProcess(Process process, IAuditor auditor) : IProcess
{
    public bool Started { get; private set; }
    public bool Terminated => process.HasExited;

    public event Action<string?>? OutputDataReceived;
    public event Action<string?>? ErrorDataReceived;

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<int>();

        void OnProcessExited(object? sender, EventArgs e)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        using var ctr = cancellationToken.Register(() =>
        {
            if (!process.HasExited)
                process.Kill();
        });

        process.Exited += OnProcessExited;
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;

        try
        {
            Started = process.Start();
            if (!Started)
            {
                auditor.WriteError("Failed to start process.");
                return -1;
            }

            if (process.StartInfo.RedirectStandardOutput)
                process.BeginOutputReadLine();

            if (process.StartInfo.RedirectStandardError)
                process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
            return await tcs.Task.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not TaskCanceledException)
        {
            auditor.WriteError($"Error running process: {ex.Message}");
            return -1;
        }
        finally
        {
            process.Exited -= OnProcessExited;
            process.OutputDataReceived -= OnOutputDataReceived;
            process.ErrorDataReceived -= OnErrorDataReceived;

            if (!cancellationToken.IsCancellationRequested)
                process.Dispose();
        }
    }

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        OutputDataReceived?.Invoke(args.Data);
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
    {
        ErrorDataReceived?.Invoke(args.Data);
    }
}