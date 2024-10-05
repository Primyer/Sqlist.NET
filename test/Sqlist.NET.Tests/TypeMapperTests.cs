using System.Data;

namespace Sqlist.NET.Tests;

public class TypeMapperTests
{
    private readonly TypeMapper _typeMapper = new TestTypeMapper(); // Use a concrete test class

    private class TestTypeMapper : TypeMapper 
    {
        // Implement the abstract methods with simple test behavior
        public override string TypeName(DbType type)
        {
            // Provide simple mappings for testing
            return type switch
            {
                DbType.Int32 => "INTEGER",
                DbType.String => "TEXT",
                DbType.Decimal => "NUMERIC",
                DbType.DateTime => "TIMESTAMP WITHOUT TIME ZONE",
                _ => type.ToString()
            };
        }

        public override Type GetType(string name)
        {
            throw new NotImplementedException();
        }
    }

    [Theory]
    [InlineData(typeof(int), "INTEGER")]
    [InlineData(typeof(string), "TEXT")]
    [InlineData(typeof(DateTime), "TIMESTAMP WITHOUT TIME ZONE")]
    public void TypeName_ShouldReturnCorrectSqlTypeName(Type type, string expectedName)
    {
        // Act
        var dbType = _typeMapper.ToDbType(type);
        var result = _typeMapper.TypeName(dbType);

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void TypeName_WithPrecisionAndScale_ShouldReturnCorrectTypeName()
    {
        // Act
        var result = _typeMapper.TypeName<decimal>(18, 2); 

        // Assert
        Assert.Equal("NUMERIC (18,2)", result);
    }

    [Fact]
    public void TypeName_WithScaleOnly_ShouldThrowInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _typeMapper.TypeName<decimal>(scale: 2)); 
    }

    [Theory]
    [InlineData(typeof(int), DbType.Int32)]
    [InlineData(typeof(string), DbType.String)]
    [InlineData(typeof(DateTime), DbType.DateTime)]
    [InlineData(typeof(object), DbType.Object)] // Unsupported type should map to DbType.Object
    public void ToDbType_ShouldReturnCorrectDbType(Type type, DbType expectedDbType)
    {
        // Act
        var result = _typeMapper.ToDbType(type);

        // Assert
        Assert.Equal(expectedDbType, result);
    }
}