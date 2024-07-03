#if DEBUG
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Handlers;
internal class TestHandler(IAuditor auditor) : TransmittableCommandHandler
{
    public override Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        auditor.WriteInformation(Resources.CommandSucceeded);
        return Task.FromResult(0);
    }
}
#endif