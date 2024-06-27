using Npgsql;

using System;

namespace Sqlist.NET.Infrastructure
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NpgsqlOptionsBuilder"/> class.
    /// </summary>
    public class NpgsqlOptionsBuilder(NpgsqlOptions options) : DbOptionsBuilder(options)
    {
        private readonly NpgsqlOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        /// <summary>
        ///     Configures the <see cref="NpgsqlDataSourceBuilder"/> that is to be used while initiating <see cref="NpgsqlDataSource"/>s.
        /// </summary>
        /// <param name="configureBuilder">The action to configure the builder.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ConfigureDataSource(Action<NpgsqlDataSourceBuilder> configureBuilder)
        {
            _options.ConfigureDataSource = configureBuilder ?? throw new ArgumentNullException(nameof(configureBuilder));
        }
    }
}
