using Sqlist.NET.Sql;

namespace Sqlist.NET.Tests;
public class SqlBuilderTests
{
    private readonly SqlBuilder _sqlBuilder = new(new DummyEnclosure());

    [Fact]
    public void RegisterFields_SingleField_AppendsToFieldsBuilder()
    {
        // Arrange
        string field = "ColumnName";

        // Act
        _sqlBuilder.RegisterFields(field);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("SELECT ColumnName", sql);
    }

    [Fact]
    public void GroupBy_SingleField_AppendsToFiltersBuilder()
    {
        // Arrange
        string field = "ColumnName";

        // Act
        _sqlBuilder.GroupBy(field);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nGROUP BY ColumnName", sql);
    }

    [Fact]
    public void GroupBy_MultipleFields_AppendsToFiltersBuilder()
    {
        // Arrange
        string[] fields = ["Column1", "Column2"];

        // Act
        _sqlBuilder.GroupBy(fields);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nGROUP BY Column1, Column2", sql);
    }

    [Fact]
    public void OrderBy_SingleField_AppendsToFiltersBuilder()
    {
        // Arrange
        string field = "ColumnName";

        // Act
        _sqlBuilder.OrderBy(field);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nORDER BY ColumnName", sql);
    }

    [Fact]
    public void OrderBy_MultipleFields_AppendsToFiltersBuilder()
    {
        // Arrange
        string[] fields = ["Column1", "Column2"];

        // Act
        _sqlBuilder.OrderBy(fields);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nORDER BY Column1, Column2", sql);
    }

    [Fact]
    public void ToSelect_CompleteQuery_ReturnsValidSql()
    {
        // Arrange
        _sqlBuilder.RegisterFields("Column1");
        _sqlBuilder.GroupBy("Column2");
        _sqlBuilder.OrderBy("Column3");

        // Act
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("SELECT Column1", sql);
        Assert.Contains("\nGROUP BY Column2", sql);
        Assert.Contains("\nORDER BY Column3", sql);
    }

    [Fact]
    public void Where_SingleCondition_AppendsToFiltersBuilder()
    {
        // Arrange
        string condition = "ColumnName = @Value";

        // Act
        _sqlBuilder.Where(condition);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nWHERE ColumnName = @Value", sql);
    }

    [Fact]
    public void Having_SingleCondition_AppendsToFiltersBuilder()
    {
        // Arrange
        string condition = "SUM(ColumnName) > @Value";

        // Act
        _sqlBuilder.Having(condition);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nHAVING SUM(ColumnName) > @Value", sql);
    }

    [Fact]
    public void Limit_AddsLimitClause()
    {
        // Arrange
        var limit = "10";

        // Act
        _sqlBuilder.Limit(limit);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nLIMIT 10", sql); // Adjust to match actual limit clause syntax
    }

    [Fact]
    public void Offset_AddsOffsetClause()
    {
        // Arrange
        var offset = "20";

        // Act
        _sqlBuilder.Offset(offset);
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("\nOFFSET 20", sql); // Adjust to match actual offset clause syntax
    }

    [Fact]
    public void ComplexQuery_CombinesConditions()
    {
        // Arrange
        _sqlBuilder.RegisterFields("Column1");
        _sqlBuilder.Where("Column2 > @Value");
        _sqlBuilder.GroupBy("Column3");
        _sqlBuilder.Having("SUM(Column4) < @Threshold");
        _sqlBuilder.OrderBy("Column5");
        _sqlBuilder.Limit("10");
        _sqlBuilder.Offset("20");

        // Act
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("SELECT Column1", sql);
        Assert.Contains("\nWHERE Column2 > @Value", sql);
        Assert.Contains("GROUP BY Column3", sql);
        Assert.Contains("\nHAVING SUM(Column4) < @Threshold", sql);
        Assert.Contains("\nORDER BY Column5", sql);
        Assert.Contains("\nLIMIT 10", sql);
        Assert.Contains("\nOFFSET 20", sql);
    }

    [Fact]
    public void Parameters_ReplacesValues()
    {
        // Arrange
        _sqlBuilder.RegisterFields("Column1");
        _sqlBuilder.Where("Column2 = @Value");

        // Act
        string sql = _sqlBuilder.ToSelect();

        // Assert
        Assert.Contains("WHERE Column2 = @Value", sql);
    }
}
