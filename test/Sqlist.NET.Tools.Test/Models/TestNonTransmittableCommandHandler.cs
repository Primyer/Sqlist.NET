using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Tests.Models;
internal class TestNonTransmittableCommandHandler : ICommandHandler
{
    public Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
