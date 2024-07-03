using Microsoft.Extensions.Logging;

namespace Sqlist.NET.Tools.Logging;
internal class AuditorLoggerProvider(IAuditor auditor) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new AuditorLogger(auditor);
    }

    public void Dispose()
    {
    }
}
