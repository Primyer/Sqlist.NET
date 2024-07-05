#if DEBUG
using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Commands;

public class TestCommand(TestHandler handler, ICommandInitializer initializer) : CommandBase<TestHandler>(handler, initializer)
{
    private readonly TestHandler _handler = handler;

    public override void Configure(CommandLineApplication app)
    {
        var command = app.Command("test", _ => { });

        _handler.Initialize(command);
        base.Configure(command);
    }
}
#endif