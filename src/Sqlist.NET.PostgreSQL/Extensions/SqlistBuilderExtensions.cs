using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Sql;

using System;

namespace Sqlist.NET.Extensions
{
    /// <summary>
    ///     Provides the extension API to configure PostgreSQL Sqlist services.
    /// </summary>
    public static class SqlistBuilderExtensions
    {
        /// <summary>
        ///     Adds PostgreSQL-specific implementations and configrations of Sqlist.NET.
        /// </summary>
        /// <param name="builder">The <see cref="SqlistBuilder"/> to configure.</param>
        /// <param name="configureOptions">The configuration action to set up the Sqlist options.</param>
        /// <returns>The <see cref="SqlistBuilder"/>.</returns>
        public static SqlistBuilder ForPostgreSQL(this SqlistBuilder builder, Action<NpgsqlOptionsBuilder>? configureOptions = null)
        {
            builder.Services.PostConfigure<NpgsqlOptions>(options =>
            {
                configureOptions?.Invoke(new NpgsqlOptionsBuilder(options));
                options.DelimitedEncloser ??= new NpgsqlEncloser();
            });

            builder.Services.TryAddSingleton<IOptions<DbOptions>>(sp => sp.GetRequiredService<IOptions<NpgsqlOptions>>());
            builder.WithContext<DbContext>();

            return builder;
        }
    }
}
