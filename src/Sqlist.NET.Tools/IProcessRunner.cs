namespace Sqlist.NET.Tools;
internal interface IProcessRunner
{
    Task<int> RunAsync(
        string executable,
        IReadOnlyList<string> args,
        string? workingDirectory = null,
        Action<string?>? handleOutput = null,
        Action<string?>? handleError = null,
        Action<string>? processCommandLine = null,
        CancellationToken cancellationToken = default);
}
