using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Tests;
internal class TestTransmittableCommandHandler : TransmittableCommandHandler
{
    public override Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}