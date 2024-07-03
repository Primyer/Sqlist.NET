using Sqlist.NET.Tools.Handlers;

namespace Sqlist.NET.Tools;

/// <summary>
///     Initializes a new instance of the <see cref="CommandInitializer"/> class.
/// </summary>
internal class CommandInitializer : ICommandInitializer
{
    public Task ExecuteAsync<THandler>(THandler handler, CancellationToken cancellationToken) where THandler : ICommandHandler
    {
        return handler.OnExecuteAsync(cancellationToken);
    }
}
