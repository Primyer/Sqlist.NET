using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools;
public interface ICommandInitializer
{
    Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler;
}
