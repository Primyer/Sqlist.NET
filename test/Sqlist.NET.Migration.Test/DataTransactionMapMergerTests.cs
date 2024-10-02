using Sqlist.NET.Data;
using Sqlist.NET.Migration.Deserialization;

namespace Sqlist.NET.Migration.Tests;

public class DataTransactionMapMergerTests
{
    [Fact]
    public void SafeMerge_ShouldHandleEmptyMaps()
    {
        // Arrange
        var maps = Enumerable.Empty<DataTransactionMap>();
        
        // Act
        var result = DataTransactionMapMerger.SafeMerge(maps);
        
        // Assert
        Assert.Empty(result.TransferDefinitions);
    }
    
    [Fact]
    public void SafeMerge_ShouldThrowArgumentNullException_WhenMapsIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataTransactionMapMerger.SafeMerge(null!));
    }
    
    [Fact]
    public void SafeMerge_ShouldMergeMapsWithoutConflicts()
    {
        // Arrange
        var map1 = CreateFakeTransferMap("Table1", "Column1");
        var map2 = CreateFakeTransferMap("Table2", "Column2");
        
        var maps = new List<DataTransactionMap> { map1, map2 };

        // Act
        var result = DataTransactionMapMerger.SafeMerge(maps);

        // Assert
        Assert.Equal(2, result.TransferDefinitions.Count);
        Assert.True(result.TransferDefinitions.ContainsKey("Table1"));
        Assert.True(result.TransferDefinitions.ContainsKey("Table2"));
    }

    [Fact]
    public void SafeMerge_ShouldThrowExceptionOnConflict()
    {
        // Arrange
        var map1 = CreateFakeTransferMap("Table1", "Column1");
        var map2 = CreateFakeTransferMap("Table1", "Column1");
        
        var maps = new List<DataTransactionMap> { map1, map2 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DataTransactionMapMerger.SafeMerge(maps));
    }

    [Fact]
    public void FullMerge_ShouldMergeMapsAndResolveConflicts()
    {
        // Arrange;
        var target = CreateFakeTransferMap("Table1", "Column1");
        var source = new DataTransactionMap
        {
            ["Table1"] = new TransactionRuleDictionary
            {
                ["Column1"] = new DataTransactionRule { ColumnName = "Column2", Type = "text" }
            }
        };
        
        // Act
        DataTransactionMapMerger.FullMerge(source, target);

        // Assert
        Assert.DoesNotContain("Column1", target.TransferDefinitions["Table1"].Columns.Keys);
        Assert.Contains("Column2", target.TransferDefinitions["Table1"].Columns.Keys);
    }

    private static DataTransactionMap CreateFakeTransferMap(string table, string column, string type = "text")
    {
        return new()
        {
            TransferDefinitions =
            {
                [table] = new DataTransferDefinition
                {
                    Script = "",
                    Columns =
                    {
                        [column] = type
                    }                        
                }
            }
        };
    }
    
    [Fact]
    public void FullMerge_ShouldThrowInvalidOperationException_WhenColumnTypeConflictsWithTransferDefinition()
    {
        // Arrange
        var target = CreateFakeTransferMap("Table1", "Column1");
        
        var source = new DataTransactionMap
        {
            ["Table1"] = new TransactionRuleDictionary
            {
                ["Column1"] = new()
                {
                    Type = "integer"
                }
            }
        };
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DataTransactionMapMerger.FullMerge(source, target));
    }
    
    [Fact]
    public void FullMerge_ShouldThrowInvalidOperationException_WhenBothOldAndNewColumnNamesExistInOneTransferDefinition()
    {
        // Arrange
        var target = CreateFakeTransferMap("Table1", "Column1");
        target.TransferDefinitions["Table1"].Columns.Add("Column2", "text");
        
        var source = new DataTransactionMap
        {
            ["Table1"] = new TransactionRuleDictionary
            {
                ["Column1"] = new()
                {
                    ColumnName = "Column2",
                    Type = "text"
                }
            }
        };
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DataTransactionMapMerger.FullMerge(source, target));
    }
}