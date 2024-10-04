using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Deserialization.Collections;
using Sqlist.NET.Migration.Exceptions;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.TestResources.Properties;
using Sqlist.NET.TestResources.Utilities;

namespace Sqlist.NET.Migration.Tests;

/// <summary>
///     Initializes a new instance of the <see cref="DataTransactionMapTests"/> class.
/// </summary>
public class DataTransactionMapTests
{
    private readonly MigrationDeserializer _deserializer = new();

    [Fact]
    public void ConstructingEmptyRoadmap_ShouldThrowException()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>(); 
        var currentVersion = new Version("1.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => new DataTransactionMap(phases, currentVersion));
    }
    
    [Fact]
    public void Constructor_ShouldSucceed_WithNullCurrentVersion()
    {
        // Arrange
        Version? currentVersion = null;
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("2.0.0") }
        };

        // Act
        var result = new DataTransactionMap(phases, currentVersion);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, phases.Count());
    }
    
    [Fact]
    public void ConstructingPhases_ShouldThrowException_WhenContainDuplicateVersions()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>
        {
            new() { Version = new Version("1.0.0") },
            new() { Version = new Version("2.0.0") },
            new() { Version = new Version("2.0.0") } // Duplicate version
        };
        var currentVersion = new Version("1.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => new DataTransactionMap(phases, currentVersion)); 
    }
    
    [Fact]
    public void ConstructingPhases_ShouldThrowException_WhenRoadmapIsEmptyAndCurrentVersionIsNotNull()
    {
        // Arrange
        IEnumerable<MigrationPhase> phases = new List<MigrationPhase>();
        var currentVersion = new Version("1.0.0");

        // Act & Assert
        Assert.Throws<MigrationException>(() => new DataTransactionMap(phases, currentVersion));
    }

    [Fact]
    public void UpdatingUndefinedTable_ShouldThrowMigrationException()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Update = { ["UndefinedTable"] = [] }
            }
        };

        Assert.Throws<MigrationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void UpdatingUndefinedColumn_ShouldThrowMigrationException()
    {
        var dataMap = new DataTransactionMap
        {
            ["SomeTable"] = []
        };
        
        var phase = new MigrationPhase
        {
            Guidelines = new PhaseGuidelines
            {
                Update =
                {
                    ["SomeTable"] = new TransactionRuleDictionary
                    {
                        ["UndefinedColumn"] = new DataTransactionRule()
                    }
                }
            }
        };

        Assert.Throws<MigrationException>(() => dataMap.Merge(phase));
    }
    [Fact]
    public void DeletingUndefinedTable_ShouldThrowMigrationException()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Delete = { ["UndefinedTable"] = [] }
            }
        };

        Assert.Throws<MigrationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void DeletingUndefinedColumn_ShouldThrowMigrationException()
    {
        var dataMap = new DataTransactionMap
        {
            ["SomeTable"] = []
        };

        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Delete = { ["SomeTable"] = ["UndefinedColumn"] }
            }
        };

        Assert.Throws<MigrationException>(() => dataMap.Merge(phase));
    }

    [Fact]
    public void TransferringDeletedColumn_ShouldThrowMigrationException()
    {
        var data = AssemblyUtility.GetEmbeddedResource(Consts.ErMigrationInitial);

        var phase1 = _deserializer.DeserializePhase(data);
        var phase2 = new MigrationPhase()
        {
            Version = new(2, 0, 0),
            Guidelines = new()
            {
                Delete = { ["Users"] = ["Id"] }
            }
        };

        phase1.Guidelines.Transfer.Add("Users", new()
        {
            Script = string.Empty,
            Columns = new()
            {
                ["Id"] = "int",
                ["Name"] = "name"
            }
        });

        var phases = new[] { phase1, phase2 };
        Assert.Throws<MigrationException>(() => new DataTransactionMap(phases));
    }

    [Fact]
    public void DeletingTable_ShouldCancelTransfer()
    {
        var data = AssemblyUtility.GetEmbeddedResource(Consts.ErMigrationInitial);

        var phase1 = _deserializer.DeserializePhase(data);
        var phase2 = new MigrationPhase()
        {
            Guidelines = new()
            {
                Delete = { ["Users"] = [] }
            }
        };

        phase1.Guidelines.Transfer.Add("Users", new()
        {
            Script = string.Empty,
            Columns = new()
            {
                ["Id"] = "int",
                ["Name"] = "name"
            }
        });

        var phases = new[] { phase1, phase2 };
        var dataMap = new DataTransactionMap(phases);

        Assert.Empty(dataMap.TransferDefinitions);
    }

    [Fact]
    public async Task MergingEmbeddedMigrationDefinitions_ShouldSucceed()
    {
        // Arrange
        var roadmapProvider = new RoadmapProvider();
        var roadmap = await roadmapProvider.GetMigrationRoadmapAsync(new MigrationAssetInfo
        {
            RoadmapAssembly = typeof(Consts).Assembly,
            RoadmapPath = Consts.RoadmapRscPath
        });
        
        // Act
        var dataMap = new DataTransactionMap(roadmap);

        // Assert
        Assert.Equal(4, dataMap.Count);
        Assert.NotNull(dataMap["Users"]["CreateDate"].Value);
        Assert.NotEmpty(dataMap.TransferDefinitions);

        foreach (var rules in dataMap.Values)
        {
            foreach (var rule in rules.Values)
            {
                Assert.NotNull(rule.CurrentType);
                Assert.NotEmpty(rule.CurrentType);
            }
        }
    }

    [Fact]
    public void UpdatingExistingColumn_ShouldSucceed()
    {
        // Arrange
        var datamap = new DataTransactionMap
        {
            ["SomeTable"] = new TransactionRuleDictionary
            {
                ["Column1"] = new() { Type = "integer" }
            }
        };

        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Update =
                {
                    ["SomeTable"] = new TransactionRuleDictionary
                    {
                        ["Column1"] = new DataTransactionRule { Type = "text" }
                    }
                }
            }
        };
        
        // Act
        datamap.Merge(phase);
        
        // Assert
        Assert.Equal("text", datamap["SomeTable"]["Column1"].Type);
    }

    [Fact]
    public void DuplicateColumns_ShouldThrowMigrationException()
    {
        // Arrange
        var datamap = new DataTransactionMap
        {
            ["SomeTable"] = new TransactionRuleDictionary
            {
                ["Column1"] = new() { Type = "integer" }
            }
        };

        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Create =
                {
                    ["SomeTable"] = new ColumnsDefinition
                    {
                        Columns = { KeyValuePair.Create("Column1", new ColumnDefinition { Type = "text" }) }
                    }
                }
            }
        };
        
        // Act & Assert
        Assert.Throws<MigrationException>(() => datamap.Merge(phase));
    }

    [Fact]
    public void MergingEmptyPhase_ShouldSucceed()
    {
        // Arrange
        var datamap = new DataTransactionMap();
        var phase = new MigrationPhase();

        // Act
        datamap.Merge(phase);
        
        // Assert
        Assert.Empty(datamap);
    }
}
