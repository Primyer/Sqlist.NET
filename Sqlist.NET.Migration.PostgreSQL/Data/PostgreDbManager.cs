using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;

namespace Sqlist.NET.Migration.Data
{
    public class PostgreDbManager : DbManager
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreDbManager"/> class.
        /// </summary>
        [ActivatorUtilitiesConstructor]
        public PostgreDbManager(DbContextBase db, IOptions<MigrationOptions> options) : base(db, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreDbManager"/> class.
        /// </summary>
        public PostgreDbManager(DbContextBase db, MigrationOptions options) : base(db, options)
        {
        }
    }
}
