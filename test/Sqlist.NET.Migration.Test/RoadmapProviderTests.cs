using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.TestResources.Properties;
using Sqlist.NET.TestResources.Utilities;

namespace Sqlist.NET.Migration.Tests;

public class RoadmapProviderTests
{
    private readonly RoadmapProvider _roadmapProvider = new RoadmapProvider();

    private readonly MigrationAssetInfo _assets = new()
    {
        RoadmapAssembly = typeof(AssemblyUtility).Assembly,
        RoadmapPath = Consts.RoadmapRscPath
    };
    
    [Fact]
    public async Task GetMigrationRoadmap_ShouldReturnListOfMigrationPhases()
    {
        // Act
        var phases = await _roadmapProvider.GetMigrationRoadmapAsync(_assets, null); 

        // Assert
        Assert.NotNull(phases);
        Assert.NotEmpty(phases);
    }

    [Fact]
    public void GetMigrationRoadmap_ShouldThrowException_WhenRoadmapAssemblyIsNull()
    {
        // Arrange
        var assets = new MigrationAssetInfo { RoadmapAssembly = null }; 

        // Act & Assert
        Assert.ThrowsAsync<MigrationException>(() => _roadmapProvider.GetMigrationRoadmapAsync(assets, null));
    }
}