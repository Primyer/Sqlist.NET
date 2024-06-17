namespace Sqlist.NET.Tools;
internal interface ICommandTransmitter
{
    Task TransmitAsync(string[] args, CancellationToken cancellationToken);
}
