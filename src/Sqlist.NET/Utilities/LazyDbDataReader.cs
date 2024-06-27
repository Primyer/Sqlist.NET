using System.Data.Common;

namespace Sqlist.NET.Utilities;
public class LazyDbDataReader
{
    public delegate void FetchEvent();

    public event FetchEvent? Fetched;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
    /// </summary>
    public LazyDbDataReader(DbDataReader reader)
    {
        Check.NotNull(reader);

        Reader = Task.FromResult(reader);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
    /// </summary>
    public LazyDbDataReader(Task<DbDataReader> lazyReader)
    {
        Check.NotNull(lazyReader, nameof(lazyReader));

        Reader = lazyReader;
    }

    public Task<DbDataReader> Reader { get; }

    public async Task IterateAsync(Action<DbDataReader> action)
    {
        using var reader = await Reader;

        while (await reader.ReadAsync())
            action.Invoke(reader);

        Fetched?.Invoke();
    }
}