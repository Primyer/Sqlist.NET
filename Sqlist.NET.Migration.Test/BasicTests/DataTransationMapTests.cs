using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration.Tests.BasicTests;
public class DataTransationMapTests
{
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
}
