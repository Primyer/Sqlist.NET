using System;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Sql;

namespace Sqlist.NET.Extensions
{
    public static class SqlistBuilderExtensions
    {
        public static SqlistBuilder ForFirebird(this SqlistBuilder builder, Action<FirebirdOptionsBuilder>? configureOptions = null)
        {
            builder.Services.PostConfigure<FirebirdOptions>(options =>
            {
                configureOptions?.Invoke(new FirebirdOptionsBuilder(options));

                options.DelimitedEnclosure ??= new FirebirdEnclosure();
            });

            builder.WithContext<DbContext>();

            builder.Services.TryAddSingleton<ISqlBuilderFactory, FirebirdBuilderFactory>();
            builder.Services.TryAddSingleton<IOptions<DbOptions>>(sp => sp.GetRequiredService<IOptions<FirebirdOptions>>());

            return builder;
        }
    }
}
