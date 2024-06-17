
using McMaster.Extensions.CommandLineUtils;

namespace Sqlist.NET.Tools.Handlers;
internal class MigrationHandler : ICommandHandler
{
    public CommandOption? Project { get; set; }
    public CommandOption? NoBuild { get; set; }

    public bool Transmittable => true;

    public Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
