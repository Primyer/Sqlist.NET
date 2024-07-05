namespace Sqlist.NET.Tools.Infrastructure;
public interface IApplicationExecutor
{
    Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
