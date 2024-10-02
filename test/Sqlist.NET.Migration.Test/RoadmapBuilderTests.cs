using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Exceptions;

namespace Sqlist.NET.Migration.Tests;

public class RoadmapBuilderTests
{
    private readonly RoadmapBuilder _roadmapBuilder = new RoadmapBuilder();

    [Fact]
    public void Build_ShouldCreateDataTransactionMap_WithOrderedPhases()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("2.0.0") },
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("3.0.0") }
        };
        var currentVersion = new Version("1.0.0");
        var targetVersion = new Version("3.0.0");

        // Act
        var result = _roadmapBuilder.Build(ref phases, currentVersion, targetVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, phases.Count()); 
        Assert.Equal(new Version("1.0.0"), phases.ElementAt(0).Version);
        Assert.Equal(new Version("2.0.0"), phases.ElementAt(1).Version);
        Assert.Equal(new Version("3.0.0"), phases.ElementAt(2).Version);
    }

    [Fact]
    public void Build_ShouldCreateDataTransactionMap_WithTargetVersionFiltering()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("2.0.0") },
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("3.0.0") }
        };
        var currentVersion = new Version("1.0.0");
        var targetVersion = new Version("2.0.0"); 

        // Act
        var result = _roadmapBuilder.Build(ref phases, currentVersion, targetVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, phases.Count()); 
        Assert.Equal(new Version("1.0.0"), phases.ElementAt(0).Version);
        Assert.Equal(new Version("2.0.0"), phases.ElementAt(1).Version); 
    }

    [Fact]
    public void Build_ShouldThrowException_WhenRoadmapIsEmpty()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>(); 
        var currentVersion = new Version("1.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => _roadmapBuilder.Build(ref phases, currentVersion));
    }
    
    [Fact]
    public void Build_ShouldCreateDataTransactionMap_WithNullCurrentVersion()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("2.0.0") }
        };
        Version? currentVersion = null; 
        var targetVersion = new Version("2.0.0");

        // Act
        var result = _roadmapBuilder.Build(ref phases, currentVersion, targetVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, phases.Count());
    }
    
    [Fact]
    public void Build_ShouldThrowException_WhenTargetVersionIsEarlierThanCurrentVersion()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("2.0.0") }
        };
        var currentVersion = new Version("2.0.0");
        var targetVersion = new Version("1.0.0"); 

        // Act & Assert
        Assert.Throws<MigrationException>(() => _roadmapBuilder.Build(ref phases, currentVersion, targetVersion)); 
    }
    
    [Fact]
    public void Build_ShouldThrowException_WhenPhasesContainDuplicateVersions()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("2.0.0") },
            new() { Version = new Version("2.0.0") } // Duplicate version
        };
        var currentVersion = new Version("1.0.0");
        var targetVersion = new Version("3.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => _roadmapBuilder.Build(ref phases, currentVersion, targetVersion)); 
    }
    
    [Fact]
    public void Build_ShouldThrowException_WhenRoadmapIsEmptyAndCurrentVersionIsNotNull()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>();
        var currentVersion = new Version("1.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => _roadmapBuilder.Build(ref phases, currentVersion));
    }
}