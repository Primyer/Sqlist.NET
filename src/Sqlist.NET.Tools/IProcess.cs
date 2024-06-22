namespace Sqlist.NET.Tools;
internal interface IProcess : IDisposable
{
    bool Started { get; }
    bool Terminated { get; }

    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
