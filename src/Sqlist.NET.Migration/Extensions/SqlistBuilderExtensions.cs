using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Migration.Infrastructure;

using System;

namespace Sqlist.NET.Migration.Extensions;
public static class SqlistBuilderExtensions
{
    /// <summary>
    ///     Adds the services and configurations of the Sqlist.NET data migration framework.
    /// </summary>
    /// <param name="builder">The <see cref="SqlistBuilder"/> to configure.</param>
    /// <param name="configureOptions">The configuration action to set up the Sqlist migration options.</param>
    /// <returns>The <see cref="SqlistBuilder"/>.</returns>
    public static SqlistBuilder WithMigration(this SqlistBuilder builder, Action<MigrationOptionsBuilder>? configureOptions = null)
    {
        builder.Services.Configure<MigrationOptions>(options =>
        {
            var optionsBuilder = new MigrationOptionsBuilder(options);
            configureOptions?.Invoke(optionsBuilder);
        });

        builder.Services.TryAddScoped<IMigrationService, MigrationService>();
        builder.Services.TryAddScoped<IMigrationContext, MigrationContext>();

        return builder;
    }
}