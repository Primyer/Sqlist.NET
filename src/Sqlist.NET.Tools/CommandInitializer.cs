using Sqlist.NET.Tools.Handlers;
using Sqlist.NET.Tools.Infrastructure;

namespace Sqlist.NET.Tools;

/// <summary>
///     Initializes a new instance of the <see cref="CommandInitializer"/> class.
/// </summary>
internal class CommandInitializer(ICommandTransmitter transmitter, IExecutionContext context) : ICommandInitializer
{
    public Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler
    {
        if (!context.IsToolContext || handler is not TransmittableCommandHandler transmittable)
            return handler.OnExecuteAsync(cancellationToken);

        return transmitter.TransmitAsync(transmittable, cancellationToken);
    }
}
