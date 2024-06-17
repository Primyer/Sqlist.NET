using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools;
internal interface ICommandInitializer
{
    Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler;
}
