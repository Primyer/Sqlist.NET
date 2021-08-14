using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Infrastructure;

using System;

namespace Sqlist.NET.Extensions
{
    /// <summary>
    ///     Provides the extension API to configure Sqlist services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the Sqlist services to the context.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="config">The configuration action to set up the Sqlist options.</param>
        public static void AddSqlist(this IServiceCollection services, Action<DbOptionsBuilder> config)
        {
            var builder = new DbOptionsBuilder();
            config.Invoke(builder);

            services.AddSingleton(builder.Options);
            services.AddScoped<DbCoreBase>();
        }
    }
}
