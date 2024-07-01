using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Sqlist.NET.Data;
using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Infrastructure;
using Sqlist.NET.TestResources.Properties;
using Sqlist.NET.TestResources.Utilities;

namespace Sqlist.NET.Migration.Tests;
/// <summary>
///     Initializes a new instance of the <see cref="DataTransationMapTests"/> class.
/// </summary>
public class DataTransationMapTests
{
    private readonly MigrationDeserializer _deserializer = new();

    [Fact]
    public void Merge_UpdateOfUndefinedTable_ShouldFail()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Update = { ["UndefinedTable"] = [] }
            }
        };

        Assert.Throws<InvalidOperationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void Merge_UpdateOfUndefinedColumn_ShouldFail()
    {
        var dataMap = new DataTransactionMap
        {
            ["SomeTable"] = []
        };

        var guidelines = new PhaseGuidelines()
        {
            Update =
            {
                ["SomeTable"] = new()
                {
                    ["UndefinedColumn"] = new DataTransactionRule()
                }
            }
        };


        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Update =
                {
                    ["SomeTable"] = new()
                    {
                        ["UndefinedColumn"] = new DataTransactionRule()
                    }
                }
            }
        };

        Assert.Throws<InvalidOperationException>(() => dataMap.Merge(phase));
    }
    [Fact]
    public void Merge_DeleteOfUndefinedTable_ShouldFail()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Delete = { ["UndefinedTable"] = [] }
            }
        };

        Assert.Throws<InvalidOperationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void Merge_DeleteOfUndefinedColumn_ShouldFail()
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

        Assert.Throws<InvalidOperationException>(() => dataMap.Merge(phase));
    }

    [Fact]
    public void Merge_TransferOfDeletedColumn_ShouldFail()
    {
        var data = AssemblyUtility.GetEmbeddedResource(Consts.ER_Migration_Intial);

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
        Assert.Throws<InvalidOperationException>(() => new DataTransactionMap(phases));
    }

    [Fact]
    public void Merge_DeletingTableShouldCancelTransfer()
    {
        var data = AssemblyUtility.GetEmbeddedResource(Consts.ER_Migration_Intial);

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
    public void Merge_ShouldSucceed()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<MigrationOptions>>();

        mockOptions.Setup(x => x.Value).Returns(new MigrationOptions()
        {
            RoadmapAssembly = typeof(Consts).Assembly,
            RoadmapPath = Consts.RoadmapRscPath
        });

        var mockMigrationContext = new Mock<MigrationContext>(
            new Mock<IDbContext>().Object,
            new Mock<IMigrationService>().Object,
            mockOptions.Object,
            new Mock<ILogger<MigrationContext>>().Object)
        {
            CallBase = true
        };

        // Act
        var roadMap = mockMigrationContext.Object.GetMigrationRoadMap();
        var dataMap = new DataTransactionMap(roadMap);

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
}
