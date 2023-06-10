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
        public static SqlistBuilder AddPostgreSQL(this SqlistBuilder builder, Action<DbOptionsBuilder>? configureOptions = null)
        {
            configureOptions?.Invoke(builder.Options);

            if (builder.Options.CaseSensitiveNaming)
                builder.WithDelimitedEncloser(builder.Options.GetOptions().DelimitedEncloser ?? new NpgsqlEncloser());

            builder.WithContext<DbContext>();
            return builder;
        }
    }
}
