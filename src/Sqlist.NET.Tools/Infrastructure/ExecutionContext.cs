namespace Sqlist.NET.Tools.Infrastructure;

/// <summary>
///     Initializes a new instance of the <see cref="ExecutionContext"/> class.
/// </summary>
internal class ExecutionContext : IExecutionContext
{
    private bool? _isTransmitter;

    public bool IsTransmitter
    {
        get => _isTransmitter ?? false;
        set
        {
            if (!_isTransmitter.HasValue)
                _isTransmitter = value;
        }
    }

    public string[] CommandLineArgs => Environment.GetCommandLineArgs();
}
