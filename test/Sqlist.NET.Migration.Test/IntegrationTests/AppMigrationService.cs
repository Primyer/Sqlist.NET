using Npgsql.NameTranslation;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Tests.Metadata;
using Sqlist.NET.Migration.Tests.Properties;

namespace Sqlist.NET.Migration.Tests.IntegrationTests;

public class AppMigrationService : MigrationService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AppMigrationService"/> class.
    /// </summary>
    public AppMigrationService() : this(CreateDbContext(), CreateMigrationOptions())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppMigrationService"/> class.
    /// </summary>
    private AppMigrationService(DbContext db, MigrationOptions? options) : base(db, options)
    {
        Db = db;
        Options = options;
    }

    public DbContext Db { get; set; }
    public MigrationOptions? Options { get; set; }

    private static DbContext CreateDbContext()
    {
        var options = new NpgsqlOptions()
        {
            ConnectionString = $"Server=localhost; Port=5432; User Id=postgres; Database={Consts.TestDatabaseName}; Password=1974563; Include Error Detail=true;",
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

