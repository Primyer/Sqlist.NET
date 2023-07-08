using Npgsql.NameTranslation;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Tests.Metadata;

using System.Data.Common;

namespace Sqlist.NET.Migration.Tests.IntegrationTests;

public class AppMigrationService : MigrationService
{
    private const string TestDatabaseName = "sqlist_net_test";

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppMigrationService"/> class.
    /// </summary>
    public AppMigrationService() : this(CreateDbContext(), CreateMigrationOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppMigrationService"/> class.
    /// </summary>
    private AppMigrationService(DbContextBase db, MigrationOptions? options) : base(db, options)
    {
        Options = options;
    }

    public MigrationOptions? Options { get; set; }

    private static DbContext CreateDbContext()
    {
        var options = new NpgsqlOptions()
        {
            ConnectionString = $"Server=localhost; Port=5432; User Id=postgres; Database={TestDatabaseName}; Password=1974563; Include Error Details=true;",
            ConfigureDataSource = builder =>
            {
                builder.MapEnum<UserStatus>("user_status", new NpgsqlNullNameTranslator());
            }
        };

        return new DbContext(options);
    }

    private static MigrationOptions CreateMigrationOptions()
    {
        var assembly = typeof(AppMigrationService).Assembly;

        return new MigrationOptionsBuilder()
            .SetMigrationAssembly(assembly, "Resources.Scripts")
            .SetDataMigrationRoadmapAssembly(assembly, "Resources.Roadmap")
            .GetOptions();
    }
}

