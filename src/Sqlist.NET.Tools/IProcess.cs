namespace Sqlist.NET.Tools;
internal interface IProcess
{
    bool Started { get; }
    bool Terminated { get; }

    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
