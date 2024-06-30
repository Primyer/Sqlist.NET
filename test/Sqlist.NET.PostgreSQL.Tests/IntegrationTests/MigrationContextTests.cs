using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Npgsql.NameTranslation;

using Sqlist.NET.Extensions;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Extensions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.Migration.Tests.Metadata;
using Sqlist.NET.Migration.Tests.Properties;
using Sqlist.NET.Sql;

using System.Collections;

using Xunit.Abstractions;

namespace Sqlist.NET.Migration.Tests.IntegrationTests;
public class MigrationContextTests
{
    private readonly ITestOutputHelper _output;
    private readonly IDbContext _db;
    private readonly IMigrationContext _migration;
    private readonly ISqlBuilderFactory _sqlFactory;
    private readonly ISchemaBuilderFactory _schemaFactory;
    private MigrationOptions? _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationContextTests"/> class.
    /// </summary>
    public MigrationContextTests(ITestOutputHelper output)
    {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSqlist()
                        .ForPostgreSQL(options =>
                        {
                            options.SetConnectionString($"Server=localhost; Port=5432; User Id=postgres; Database={Consts.TestDatabaseName}; Password=1974563; Include Error Detail=true;");
                            options.ConfigureDataSource(builder =>
                            {
                                builder.MapEnum<UserStatus>("user_status", new NpgsqlNullNameTranslator());
                            });
                        })
                        .AddSqlistMigration(options =>
                        {
                            var assembly = typeof(Consts).Assembly;

                            options.SetMigrationAssembly(assembly, Consts.ScriptsRscPath);
                            options.SetDataMigrationRoadmapAssembly(assembly, Consts.RoadmapRscPath);

                            _options = options.GetOptions();
                        });
            })
            .Build();


        _output = output;

        _db = host.Services.GetRequiredService<IDbContext>();
        _migration = host.Services.GetRequiredService<IMigrationContext>();
        _sqlFactory = host.Services.GetRequiredService<ISqlBuilderFactory>();
        _schemaFactory = host.Services.GetRequiredService<ISchemaBuilderFactory>();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnInformation()
    {
        var info = await _migration.InitializeAsync(null);

        _output.WriteLine("Current version      = " + info.CurrentVersion);
        _output.WriteLine("Migration to version = " + info.LatestVersion);
        _output.WriteLine("Title                = " + info.Title);
        _output.WriteLine("Description          = " + info.Description);
        _output.WriteLine("Schema changes       =\n" + info.SchemaChanges);
    }

    [Theory, ClassData(typeof(MigrationData))]
    public async Task MigrateDataAsync(Version? currentVersion, Version targetVersion)
    {
        _options!.ScriptsPath = "Resources.Scripts.v" + targetVersion.Major;

        var info = await _migration.InitializeAsync(targetVersion);

        Assert.Equal(currentVersion, info.CurrentVersion);
        Assert.Equal(targetVersion, info.TargetVersion);

        await _migration.MigrateDataAsync();
    }

    [Fact]
    public async Task MigrateDataAsync_ShouldSucceed()
    {
        try
        {
            _options!.ScriptsPath = "Resources.Scripts.v1";

            await _migration.InitializeAsync(new(1, 0, 0));
            await _migration.MigrateDataAsync();

            _options!.ScriptsPath = "Resources.Scripts.v3";

            await _migration.InitializeAsync(new(3, 0, 0));
            await _migration.MigrateDataAsync();
        }
        finally
        {
            await ResetDatabase();
        }
    }

    private async Task ResetDatabase()
    {
        await _db.TerminateDatabaseConnectionsAsync(Consts.TestDatabaseName);

        await DeleteTestDatabases();
        await CreateTestDatabase();
    }

    private async Task CreateTestDatabase()
    {
        var sql = _schemaFactory.Create().CreateDatabase(Consts.TestDatabaseName);

        await _db.Query().ExecuteAsync(sql);
        await _db.Connection!.ChangeDatabaseAsync(Consts.TestDatabaseName);
    }

    private async Task DeleteTestDatabases()
    {
        var sql = _sqlFactory.Sql("pg_database");

        sql.RegisterFields("datname");
        sql.Where("datistemplate = false");
        sql.AppendAnd($"datname like '{Consts.TestDatabaseName}%'");

        var stmt = sql.ToSelect();
        var databases = await _db.Query().RetrieveAsync<string>(stmt);

        foreach (var database in databases)
        {
            stmt = _schemaFactory.Create().DeleteDatabase(database);
            await _db.Query().ExecuteAsync(stmt);
        }
    }

    private class MigrationData : IEnumerable<object[]>
    {
        private readonly List<object?[]> _data =
        [
            [null, new Version(1, 0, 0)],
            [new Version(1, 0, 0), new Version(2, 0, 0)],
            [new Version(2, 0, 0), new Version(3, 0, 0)]
        ];

        public IEnumerator<object[]> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
