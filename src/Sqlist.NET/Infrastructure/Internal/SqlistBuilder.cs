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
        internal SqlistBuilder(IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));
            Services = services;
        }

        public IServiceCollection Services { get; }

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
