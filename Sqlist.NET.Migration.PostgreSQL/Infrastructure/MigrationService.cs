using DbUp;
using DbUp.Builder;

using Microsoft.Extensions.Options;

using Sqlist.NET.Migration.Data;

namespace Sqlist.NET.Migration.Infrastructure
{
    public class MigrationService : MigrationServiceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationService"/> class.
        /// </summary>
        public MigrationService(PostgreDbManager db, IOptions<MigrationOptions> options) : base(db, options)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigrationService"/> class.
        /// </summary>
        public MigrationService(PostgreDbManager db, MigrationOptions? options) : base(db, options)
        {
        }

        /// <inheritdoc />
        protected override UpgradeEngineBuilder InitializeUpgradeEngine()
        {
            return DeployChanges.To.PostgresqlDatabase(_db.Options.ConnectionString);
        }
    }
}
