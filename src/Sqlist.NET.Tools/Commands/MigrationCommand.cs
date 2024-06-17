using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Commands;

/// <summary>
///     Initializes a new instance of the <see cref="MigrationCommand"/> class.
/// </summary>
internal class MigrationCommand(MigrationHandler handler, ICommandInitializer initializer) : CommandBase<MigrationHandler>(handler, initializer)
{
    private readonly MigrationHandler _handler = handler;

    public override void Configure(CommandLineApplication command)
    {
        if (Configured) return;

        _handler.Project = command.Option("-p|--project <PROJECT>", Resources.ProjectDescription, CommandOptionType.SingleValue);
        _handler.NoBuild = command.Option("--no-build", Resources.NoBuildDescription, CommandOptionType.NoValue);

        base.Configure(command);
    }
}