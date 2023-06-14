using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;
using Sqlist.NET.Migration.Tests.IntegrationTests;

namespace Sqlist.NET.Migration.Tests.BasicTests;
public class DataTransationMapTests : IClassFixture<AppMigrationService>
{
    private readonly AppMigrationService _service;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTransationMapTests"/> class.
    /// </summary>
    public DataTransationMapTests(AppMigrationService service)
    {
        _service = service;
    }

    [Fact]
    public void Merge_UpdateOfUndefinedTable_ShouldFail()
    {
        var phase = new MigrationPhase
        {
            Guidelines = new PhaseGuidelines
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

        var guidelines = new PhaseGuidelines
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
            Guidelines = new PhaseGuidelines
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
            Guidelines = new PhaseGuidelines
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
            Guidelines = new PhaseGuidelines
            {
                Delete = { ["SomeTable"] = new[] { "UndefinedColumn" } }
            }
        };

        Assert.Throws<InvalidOperationException>(() => dataMap.Merge(phase));
    }

    [Fact]
    public void Merge_ShouldSucceed()
    {
        var roadMap = _service.GetMigrationRoadMap();
        var dataMap = new DataTransactionMap(roadMap);
        
        Assert.Equal(3, dataMap.Count);
        Assert.NotNull(dataMap["Users"]["CreateDate"].Value);

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
