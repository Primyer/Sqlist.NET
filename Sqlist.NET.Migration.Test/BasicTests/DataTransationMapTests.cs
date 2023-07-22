using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Tests.IntegrationTests;
using Sqlist.NET.Migration.Tests.Properties;
using Sqlist.NET.Migration.Tests.Utilities;

namespace Sqlist.NET.Migration.Tests.BasicTests;
public class DataTransationMapTests : IClassFixture<AppMigrationService>
{
    private readonly AppMigrationService _service;
    private readonly MigrationDeserializer _deserializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTransationMapTests"/> class.
    /// </summary>
    public DataTransationMapTests(AppMigrationService service)
    {
        _service = service;
        _deserializer = new MigrationDeserializer();
    }

    [Fact]
    public void Merge_UpdateOfUndefinedTable_ShouldFail()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Update = { ["UndefinedTable"] = new() }
            }
        };

        Assert.Throws<InvalidOperationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void Merge_UpdateOfUndefinedColumn_ShouldFail()
    {
        var dataMap = new DataTransactionMap
        {
            ["SomeTable"] = new()
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
                Delete = { ["UndefinedTable"] = Array.Empty<string>() }
            }
        };

        Assert.Throws<InvalidOperationException>(() => new DataTransactionMap().Merge(phase));
    }

    [Fact]
    public void Merge_DeleteOfUndefinedColumn_ShouldFail()
    {
        var dataMap = new DataTransactionMap
        {
            ["SomeTable"] = new()
        };

        var phase = new MigrationPhase
        {
            Guidelines = new()
            {
                Delete = { ["SomeTable"] = new[] { "UndefinedColumn" } }
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
            Guidelines = new()
            {
                Delete = { ["Users"] = new[] { "Id" } }
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
                Delete = { ["Users"] = Array.Empty<string>() }
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
        var roadMap = _service.GetMigrationRoadMap();
        var dataMap = new DataTransactionMap(roadMap);

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
