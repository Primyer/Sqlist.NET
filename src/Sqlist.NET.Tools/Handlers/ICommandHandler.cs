namespace Sqlist.NET.Tools.Handlers;
internal interface ICommandHandler
{
    Task<int> OnExecuteAsync(CancellationToken cancellationToken);
}
