namespace Sqlist.NET.Tools.Cli;
internal interface IProcess
{
    bool Started { get; }
    bool Terminated { get; }

    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
