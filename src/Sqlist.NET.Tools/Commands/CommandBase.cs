using McMaster.Extensions.CommandLineUtils;

using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Logging;
using Sqlist.NET.Tools.Properties;

namespace Sqlist.NET.Tools.Commands;

public delegate void CommandCompletionEvent();

/// <summary>
///     Initializes a new instance of the <see cref="CommandBase{THandler}"/> class.
/// </summary>
internal abstract class CommandBase : ICommand
{
    protected CommandOption? Verbose { get; set; }
    protected CommandOption? NoColor { get; set; }
    protected CommandOption? PrefixOutput { get; set; }

    public bool Configured { get; private set; }

    public virtual void Configure(CommandLineApplication app)
    {
        if (Configured) return;

        Verbose = app.Option("-v|--verbose", Resources.VerboseDescription, CommandOptionType.NoValue);
        NoColor = app.Option("--no-color", Resources.NoColorDescription, CommandOptionType.NoValue);
        PrefixOutput = app.Option("--prefix-output", Resources.PrefixDescription, CommandOptionType.NoValue);

        Configured = true;
    }
}

/// <summary>
///     Initializes a new instance of the <see cref="CommandBase{THandler}"/> class.
/// </summary>
internal abstract class CommandBase<THandler>(THandler handler, ICommandInitializer initializer) : CommandBase where THandler : ICommandHandler
{
    public event CommandCompletionEvent OnCompleted = () => { };

    public override void Configure(CommandLineApplication app)
    {
        if (Configured) return;

        app.OnExecuteAsync(ExecuteAsync);
        base.Configure(app);
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Reporter.NoColor = NoColor?.HasValue() ?? false;
        Reporter.IsVerbose = Verbose?.HasValue() ?? false;
        Reporter.PrefixOutput = PrefixOutput?.HasValue() ?? false;

        Validate();

        await initializer.ExecuteAsync(handler, cancellationToken);
        OnCompleted?.Invoke();
    }

    protected virtual void Validate() { }
}