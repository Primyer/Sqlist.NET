namespace Sqlist.NET.Tools.Handlers;
internal interface ICommandHandler
{
    bool Transmittable { get; }
    Task<int> OnExecuteAsync(CancellationToken cancellationToken);
}
