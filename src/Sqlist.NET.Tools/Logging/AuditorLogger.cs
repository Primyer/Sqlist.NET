using Microsoft.Extensions.Logging;

namespace Sqlist.NET.Tools.Logging;

/// <summary>
///     Initializes a new instaince of the <see cref="AuditorLogger"/> class.
/// </summary>
internal class AuditorLogger(IAuditor auditor) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel)
    {
        return IAuditor.IsVerbose || LogLevel.Trace < logLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        switch (logLevel)
        {
            case LogLevel.Trace:
                auditor.WriteVerbose(message);
                break;

            case LogLevel.Debug:
                auditor.WriteData(message);
                break;

            case LogLevel.Information:
                auditor.WriteInformation(message);
                break;

            case LogLevel.Warning:
                auditor.WriteWarning(message);
                break;

            case LogLevel.Error:
            case LogLevel.Critical:
                auditor.WriteError(message);
                break;

            default: break;
        }
    }
}
