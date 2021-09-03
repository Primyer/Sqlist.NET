using Sqlist.NET.Utilities;

using System;
using System.Threading.Tasks;

using ado = System.Data.Common;

namespace Sqlist.NET.Common
{
    public class LazyDbDataReader
    {
        private Task<ado::DbDataReader> _lazyReader;

        public delegate void FetchEvent();

        public event FetchEvent Fetched;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
        /// </summary>
        public LazyDbDataReader(ado::DbDataReader reader)
        {
            Check.NotNull(reader, nameof(reader));

            _lazyReader = Task.FromResult(reader);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LazyDbDataReader"/> class.
        /// </summary>
        public LazyDbDataReader(Task<ado::DbDataReader> lazyReader)
        {
            Check.NotNull(lazyReader, nameof(lazyReader));

            _lazyReader = lazyReader;
        }

        public async Task IterateAsync(Action<ado::DbDataReader> action)
        {
            using var reader = await _lazyReader;

            while (await reader.ReadAsync())
                action.Invoke(reader);

            Fetched?.Invoke();
        }

        public async Task<ado::DbDataReader> GetReaderAsync()
        {
            var reader = await _lazyReader;
            _lazyReader = Task.FromResult(reader);

            return reader;
        }
    }
}
