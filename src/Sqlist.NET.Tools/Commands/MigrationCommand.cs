using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Commands;

/// <summary>
///     Initializes a new instance of the <see cref="MigrationCommand"/> class.
/// </summary>
internal class MigrationCommand(MigrationHandler handler, ICommandInitializer initializer) : CommandBase<MigrationHandler>(handler, initializer)
{
    private readonly MigrationHandler _handler = handler;

    public override void Configure(CommandLineApplication app)
    {
        var command = app.Command("migrate", _ => { });

        _handler.Initialize(command);
        base.Configure(command);
    }
}