using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools.Cli;
internal interface ICommandTransmitter
{
    Task TransmitAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : TransmittableCommandHandler;
}
