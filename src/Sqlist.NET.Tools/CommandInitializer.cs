using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools;

/// <summary>
///     Initializes a new instance of the <see cref="CommandInitializer"/> class.
/// </summary>
internal class CommandInitializer(ICommandTransmitter transmitter) : ICommandInitializer
{
    static readonly string[] _cmdArgs = Environment.GetCommandLineArgs();

    public Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler
    {
        if (!handler.Transmittable)
            return handler.OnExecuteAsync(cancellationToken);

        return transmitter.TransmitAsync(_cmdArgs, cancellationToken);
    }
}
