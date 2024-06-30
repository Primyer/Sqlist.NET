using Sqlist.NET.Utilities;

namespace Sqlist.NET.Tests;
public class CheckTests
{
    interface IInterface { }
    abstract class Abstract { }

    [Theory]
    [InlineData(typeof(IInterface))]
    [InlineData(typeof(Abstract))]
    public void Instantiable_Throws_WhenTypeUninstantiable(Type type)
    {
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            Check.Instantiable(type);
        });
    }

    [Fact]
    public void Instantiable_DoesntThrow_WhenValid()
    {
        // Arrange
        var type = typeof(CheckTests);

        // Act
        Check.Instantiable(type);
    }

    [Fact]
    public void NotNull_Throws_WhenArgumentIsNull()
    {
        // Arrange
        string? str = null;

        // Assert
        Assert.Throws<ArgumentNullException>(nameof(str), () =>
        {
            Check.NotNull(str);
        });
    }

    [Fact]
    public void NotNull_DoesntThrow_WhenArgumentNotNull()
    {
        // Arrange
        var str = string.Empty;

        // Act
        Check.NotNull(str);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NotNullOrEmpty_Throws_WhenArgumentNullOrEmpty(string? value)
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Check.NotNullOrEmpty(value);
        });
    }

    [Fact]
    public void NotNullOrEmpty_DoesntThrow_WhenArgumentNotNullOrEmpty()
    {
        // Arrange
        var value = "Hello!";

        // Act
        Check.NotNullOrEmpty(value);
    }

    public static IEnumerable<object[]> Collections => [
        [ new int[] { 1, 2, 3 } ],
        [ new List<long> { 1, 2, 3 } ],
        [ "Hello!" ],
    ];

    public static IEnumerable<object[]> EmptyCollections => [
        [ Enumerable.Empty<int>() ],
        [ Array.Empty<string>() ],
        [ new List<long>() ],
        [ "" ],
    ];

    [Theory]
    [MemberData(nameof(EmptyCollections))]
    public void NotEmpty_Throws_WhenArgumentEmpty<T>(IEnumerable<T> collection)
    {
        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Check.NotEmpty(collection);
        });
    }

    [Theory]
    [MemberData(nameof(Collections))]
    public void NotEmpty_DoesntThrow_WhenArgumentNotEmpty<T>(IEnumerable<T> collection)
    {
        // Act
        Check.NotEmpty(collection);
    }
}
