using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Exceptions;
using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Commands;

/// <summary>
///     Initializes a new instance of the <see cref="MigrationCommand"/> class.
/// </summary>
public class MigrationCommand(MigrationHandler handler, ICommandInitializer initializer) : CommandBase<MigrationHandler>(handler, initializer)
{
    private readonly MigrationHandler _handler = handler;

    public CommandOption? FromVersion { get; set; }
    public CommandOption? ToVersion { get; set; }


    public override void Configure(CommandLineApplication app)
    {
        var command = app.Command("migrate", _ => { });

        _handler.Initialize(command);

        FromVersion = command.Option("--from-version", Resources.FromVersionDescription, CommandOptionType.SingleValue);
        ToVersion = command.Option("--to-version", Resources.ToVersionDescription, CommandOptionType.SingleValue);

        base.Configure(command);
    }

    protected override void Validate()
    {
        if (FromVersion!.HasValue())
        {
            var value = FromVersion.Value() ?? string.Empty;

            if (!Version.TryParse(value, out var version))
                throw new InvalidOptionException(FromVersion);

            _handler.FromVersion = version;
        }

        if (ToVersion!.HasValue())
        {
            var value = ToVersion.Value() ?? string.Empty;

            if (!Version.TryParse(value, out var version))
                throw new InvalidOptionException(ToVersion);

            _handler.ToVersion = version;
        }

        base.Validate();
    }
}