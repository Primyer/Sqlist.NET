using System.Data.Common;

namespace Sqlist.NET.Utilities;

public delegate void FetchEvent();

public interface ILazyDataReader
{
    event FetchEvent? Fetched;
    Task<DbDataReader> Reader { get; }
    Task IterateAsync(Action<DbDataReader> action, CancellationToken cancellationToken = default);
}
