using System.Collections;

using Xunit.Abstractions;

namespace Sqlist.NET.Migration.Tests.IntegrationTests;
public class MigrationServiceTests : IClassFixture<AppMigrationService>
{
    private readonly AppMigrationService _service;
    private readonly ITestOutputHelper _output;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MigrationServiceTests"/> class.
    /// </summary>
    public MigrationServiceTests(AppMigrationService service, ITestOutputHelper output)
    {
        ThrowIfNull(output);

        _service = service;
        _output = output;
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnInformation()
    {
        var info = await _service.InitializeAsync(null);

        _output.WriteLine("Current version      = " + info.CurrentVersion);
        _output.WriteLine("Migration to version = " + info.LatestVersion);
        _output.WriteLine("Title                = " + info.Title);
        _output.WriteLine("Description          = " + info.Description);
        _output.WriteLine("Schema changes       =\n" + info.SchemaChanges);
    }

    [Theory, ClassData(typeof(MigrationData))]
    public async Task MigrateDataAsync_ShouldMigrationDatabaseSeccessfully(Version? currentVersion, Version targetVersion)
    {
        _service.Options!.ScriptsPath = "Resources.Scripts.v" + targetVersion.Major;

        var info = await _service.InitializeAsync(targetVersion);

        Assert.Equal(currentVersion, info.CurrentVersion);
        Assert.Equal(targetVersion, info.TargetVersion);

        await _service.MigrateDataAsync();
    }

    private class MigrationData : IEnumerable<object[]>
    {
        private readonly List<object?[]> _data = new()
        {
            new object?[] { null, new Version(1, 0, 0) },
            new object[] { new Version(1, 0, 0), new Version(2, 0, 0) },
            new object[] { new Version(2, 0, 0), new Version(3, 0, 0) }
        };

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
