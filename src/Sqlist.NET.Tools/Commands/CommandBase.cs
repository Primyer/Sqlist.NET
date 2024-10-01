using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Commands;

/// <summary>
///     Initializes a new instance of the <see cref="CommandBase{THandler}"/> class.
/// </summary>
public abstract class CommandBase : ICommand
{
    public CommandOption? Verbose { get; private set; }
    public CommandOption? NoColor { get; private set; }
    public CommandOption? PrefixOutput { get; private set; }

    public virtual void Configure(CommandLineApplication app)
    {
        Verbose = app.Option("-v|--verbose", Resources.VerboseDescription, CommandOptionType.NoValue);
        NoColor = app.Option("--no-color", Resources.NoColorDescription, CommandOptionType.NoValue);
        PrefixOutput = app.Option("--prefix-output", Resources.PrefixDescription, CommandOptionType.NoValue);
    }
}

/// <summary>
///     Initializes a new instance of the <see cref="CommandBase{THandler}"/> class.
/// </summary>
public abstract class CommandBase<THandler>(THandler handler, ICommandInitializer initializer) : CommandBase where THandler : ICommandHandler
{
    public override void Configure(CommandLineApplication app)
    {
        app.OnExecuteAsync(ExecuteAsync);
        base.Configure(app);
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Auditor.NoColor = NoColor?.HasValue() ?? false;
        Auditor.IsVerbose = Verbose?.HasValue() ?? false;
        Auditor.PrefixOutput = PrefixOutput?.HasValue() ?? false;

        Validate();

        return initializer.ExecuteAsync(handler, cancellationToken);
    }

    protected virtual void Validate() { }
}