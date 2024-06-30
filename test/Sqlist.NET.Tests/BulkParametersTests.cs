using Sqlist.NET.Utilities;

namespace Sqlist.NET.Tests;
public class BulkParametersTests
{
    private class TestClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private class TestClassWithNoProperties
    {
    }

    [Fact]
    public void BulkParameters_Enumerates_Correctly()
    {
        // Arrange
        var objects = new List<object>
        {
            new TestClass { Name = "Alice", Age = 30 },
            new TestClass { Name = "Bob", Age = 25 }
        };

        var expected = new List<KeyValuePair<object?, Type>[]>
        {
            new KeyValuePair<object?, Type>[]
            {
                KeyValuePair.Create((object?)"Alice", typeof(string)),
                KeyValuePair.Create((object?)30, typeof(int))
            },
            new KeyValuePair<object?, Type>[]
            {
                KeyValuePair.Create((object?)"Bob", typeof(string)),
                KeyValuePair.Create((object?)25, typeof(int))
            }
        };

        // Act
        var bulkParameters = new BulkParameters(objects);

        // Assert
        Assert.Equal(expected, [.. bulkParameters]);
    }

    [Fact]
    public void BulkParameters_Handles_EmptyCollection()
    {
        // Arrange
        var objects = new List<object>();

        // Act
        var bulkParameters = new BulkParameters(objects);

        // Assert
        Assert.Empty(bulkParameters);
    }

    [Fact]
    public void BulkParameters_Handles_ObjectsWithNoProperties()
    {
        // Arrange
        var objects = new List<object>
        {
            new TestClassWithNoProperties()
        };

        var expected = new List<KeyValuePair<object?, Type>[]>
        {
            Array.Empty<KeyValuePair<object?, Type>>()
        };

        // Act
        var bulkParameters = new BulkParameters(objects);

        // Assert
        Assert.Equal(expected, [.. bulkParameters]);
    }
}
