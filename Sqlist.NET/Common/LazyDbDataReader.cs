using Sqlist.NET.Utilities;

using System;
using System.Threading.Tasks;

using ado = System.Data.Common;

namespace Sqlist.NET.Common
{
    public class LazyDbDataReader
    {
        public delegate void FetchEvent();

        public event FetchEvent Fetched;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
        /// </summary>
        public LazyDbDataReader(ado::DbDataReader reader)
        {
            Check.NotNull(reader, nameof(reader));

            Reader = Task.FromResult(reader);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
        /// </summary>
        public LazyDbDataReader(Task<ado::DbDataReader> lazyReader)
        {
            Check.NotNull(lazyReader, nameof(lazyReader));

            Reader = lazyReader;
        }

        public Task<ado::DbDataReader> Reader { get; }

        public async Task IterateAsync(Action<ado::DbDataReader> action)
        {
            using var reader = await Reader;

            while (await reader.ReadAsync())
                action.Invoke(reader);

            Fetched?.Invoke();
        }
    }
}
