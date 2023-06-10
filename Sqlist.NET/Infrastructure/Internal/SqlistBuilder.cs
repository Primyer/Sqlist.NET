using Microsoft.Extensions.DependencyInjection;

using Sqlist.NET.Sql;
using Sqlist.NET.Utilities;

namespace Sqlist.NET.Infrastructure.Internal
{
    public readonly struct SqlistBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlistBuilder"/> struct.
        /// </summary>
        /// <param name="services"></param>
        internal SqlistBuilder(IServiceCollection services, DbOptionsBuilder options)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(options, nameof(options));

            Services = services;
            Options = options;
        }

        public IServiceCollection Services { get; }
        public DbOptionsBuilder Options { get; }

        public void WithContext<T>() where T : DbContextBase
        {
            Services.AddScoped<T>();
            Services.AddScoped<DbContextBase, T>(sp => sp.GetRequiredService<T>());
        }

        public void WithDelimitedEncloser(Encloser encloser)
        {
            Encloser.Default = encloser;
        }
    }
}
