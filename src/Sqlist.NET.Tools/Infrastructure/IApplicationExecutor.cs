namespace Sqlist.NET.Tools.Infrastructure;
internal interface IApplicationExecutor
{
    Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
