namespace Sqlist.NET.Tools;
internal interface IProcessManager
{
    IProcess Prepare(
        string executable,
        IReadOnlyList<string> args,
        string? workingDirectory = null,
        Action<string?>? handleOutput = null,
        Action<string?>? handleError = null,
        Action<string>? processCommandLine = null);
}
