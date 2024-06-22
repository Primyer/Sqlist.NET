using Microsoft.Extensions.Logging;

using Sqlist.NET.Tools;

using System.Diagnostics;

internal class ManagedProcess(Process process, ILogger logger) : IProcess
{
    public bool Started { get; private set; }
    public bool Terminated => process.HasExited;

    public event Action<string?>? OutputDataReceived;
    public event Action<string?>? ErrorDataReceived;

    public void Dispose()
    {
        ((IDisposable)process).Dispose();
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tcs = new TaskCompletionSource<int>();

        void OnProcessExited(object? sender, EventArgs e)
        {
            tcs.TrySetResult(process.ExitCode);
        }

        process.Exited += OnProcessExited;
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;

        try
        {
            using var ctr = cancellationToken.Register(() =>
            {
                if (!process.HasExited)
                    process.Kill();
            });

            Started = process.Start();

            if (process.StartInfo.RedirectStandardOutput)
                process.BeginOutputReadLine();

            if (process.StartInfo.RedirectStandardError)
                process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
            return await tcs.Task.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError("Error running process: {Message}", ex.Message);
            return -1;
        }
        finally
        {
            process.Exited -= OnProcessExited;
            process.OutputDataReceived -= OnOutputDataReceived;
            process.ErrorDataReceived -= OnErrorDataReceived;
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