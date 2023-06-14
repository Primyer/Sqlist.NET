using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;

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
        return new DbContext(new(), cs =>
        {
            cs.Host = "localhost";
            cs.Port = 5432;
            cs.Username = "postgres";
            cs.Database = TestDatabaseName;
            cs.Password = "1974563";
            cs.IncludeErrorDetail = true;
        });
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

