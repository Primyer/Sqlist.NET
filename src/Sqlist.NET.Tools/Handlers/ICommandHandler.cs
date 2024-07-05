namespace Sqlist.NET.Tools.Handlers;
public interface ICommandHandler
{
    Task<int> OnExecuteAsync(CancellationToken cancellationToken);
}
