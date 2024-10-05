using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Infrastructure.Internal;

namespace Sqlist.NET.Extensions;

/// <summary>
///     Provides the extension API to configure Sqlist services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the Sqlist services to the context.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configureOptions">The configuration action to set up the Sqlist options.</param>
    /// <returns>The <see cref="SqlistBuilder"/>.</returns>
    public static SqlistBuilder AddSqlist(this IServiceCollection services, Action<DbOptionsBuilder>? configureOptions = null)
    {
        var configure = new ConfigureNamedOptions<DbOptions>(null,
            options => configureOptions?.Invoke(new DbOptionsBuilder(options)));

        services.TryAddSingleton<IConfigureOptions<DbOptions>>(configure);
        services.TryAddScoped<ITransactionManager, TransactionManager>();

        return new SqlistBuilder(services);
    }
}