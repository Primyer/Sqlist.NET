using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools;
internal interface ICommandTransmitter
{
    Task TransmitAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : TransmittableCommandHandler;
}
