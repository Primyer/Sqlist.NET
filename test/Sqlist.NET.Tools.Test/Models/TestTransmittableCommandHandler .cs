using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Tests;
internal class TestTransmittableCommandHandler : TransmittableCommandHandler
{
    public CommandOption? TestOption { get; private set; }

    public void Configure(CommandLineApplication command)
    {
        TestOption = command.Option("-t|--test <TEST>", "Test option", CommandOptionType.SingleValue);
        Initialize(command);
    }

    public override Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}