using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Cli;

/// <summary>
///     Initializes a new instance of the <see cref="CliCommandInitializer"/> class.
/// </summary>
internal class CliCommandInitializer(ICommandTransmitter transmitter) : ICommandInitializer
{
    public Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler
    {
        if (handler is not TransmittableCommandHandler transmittable)
            return handler.OnExecuteAsync(cancellationToken);

        return transmitter.TransmitAsync(transmittable, cancellationToken);
    }
}
