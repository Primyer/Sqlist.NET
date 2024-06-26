﻿using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Infrastructure.Internal;
using Sqlist.NET.Migration.Data;
using Sqlist.NET.Migration.Infrastructure;

using System;

namespace Sqlist.NET.Migration.Extensions
{
    public static class SqlistBuilderExtensions
    {
        /// <summary>
        ///     Adds the services and configurations of the Sqlist.NET data migration framework.
        /// </summary>
        /// <param name="builder">The <see cref="SqlistBuilder"/> to configure.</param>
        /// <param name="configureOptions">The configuration action to set up the Sqlist migration options.</param>
        /// <returns>The <see cref="SqlistBuilder"/>.</returns>
        public static SqlistBuilder AddSqlistMigration(this SqlistBuilder builder, Action<MigrationOptionsBuilder> configureOptions)
        {
            var options = new MigrationOptionsBuilder();
            configureOptions(options);

            builder.Services.ConfigureOptions(options.GetOptions());

            builder.Services.AddScoped<DbManager>();
            builder.Services.AddTransient<MigrationService>();

            return builder;
        }
    }
}
