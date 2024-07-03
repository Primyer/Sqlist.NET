using Moq;

using Sqlist.NET.Annotations;
using Sqlist.NET.Metadata;
using Sqlist.NET.Properties;
using Sqlist.NET.Serialization;
using Sqlist.NET.Utilities;

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;

namespace Sqlist.NET.Tests;
public class DataSerializerTests
{
    [Fact]
    public async Task Json_DeserializesJsonStrings()
    {
        // Arrange
        string[] jsonData = [
            """{"Name": "John"}""",
            """{"Name": "Doe" }"""
        ];

        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.SetupSequence(r => r.GetString(0))
                  .Returns(jsonData[0])
                  .Returns(jsonData[1]);

        // Act
        var result = await DataSerializer.Json<TestModel>(lazyReader);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("John", result.First().Name);
        Assert.Equal("Doe", result.Last().Name);
    }

    [Fact]
    public async Task Json_HandlesEmptyReader()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.Setup(r => r.ReadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await DataSerializer.Json<TestModel>(lazyReader);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Primitive_ConvertsPrimitiveTypes()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.SetupSequence(r => r.GetValue(0))
                  .Returns(42)
                  .Returns(100);

        // Act
        var result = await DataSerializer.Primitive<int>(lazyReader);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(42, result.First());
        Assert.Equal(100, result.Last());
    }

    [Fact]
    public void GetObjectOrientedNames_ReturnsCorrectSerializationFields()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();

        mockReader.Setup(r => r.FieldCount).Returns(3);
        mockReader.Setup(r => r.GetOrdinal("name_column")).Returns(0);
        mockReader.Setup(r => r.GetOrdinal("age_column")).Returns(1);
        mockReader.Setup(r => r.GetOrdinal("LastName")).Returns(2);

        // Act
        var result = DataSerializer.GetObjectOrientedNames<TestModelWithCustomNames>(mockReader.Object);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("Name", result[0].Name);
        Assert.Equal("Age", result[1].Name);
        Assert.Equal("LastName", result[2].Name);
    }

    [Fact]
    public void GetObjectOrientedNames_Throws_WhenQueryResultDontMatchProperties()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();

        mockReader.Setup(r => r.FieldCount).Returns(2);
        mockReader.Setup(r => r.GetName(0)).Returns("name_column");
        mockReader.Setup(r => r.GetName(1)).Returns("undefined");

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            DataSerializer.GetObjectOrientedNames<TestModelWithCustomNames>(mockReader.Object);
        });

        Assert.Equal(Resources.InvalidObjectOrientedProperties, exception.Message);
    }

    [Fact]
    public void GetObjectOrientedNames_HandlesEmptyReader()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        mockReader.Setup(r => r.FieldCount).Returns(0);

        // Act
        var result = DataSerializer.GetObjectOrientedNames<TestModel>(mockReader.Object);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Serialize_HandlesNullProperty()
    {
        // Arrange
        PropertyInfo? property = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DataSerializer.Serialize(property!));
    }

    [Fact]
    public void GetQueryOrientedNames_ReturnsCorrectSerializationFields()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        mockReader.Setup(r => r.FieldCount).Returns(2);
        mockReader.Setup(r => r.GetName(0)).Returns("name_column");
        mockReader.Setup(r => r.GetName(1)).Returns("age_column");

        // Act
        var result = DataSerializer.GetQueryOrientedNames<TestModelWithCustomNames>(mockReader.Object);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("Name", result[0].Name);
        Assert.Equal("Age", result[1].Name);
    }

    [Fact]
    public void GetQueryOrientedNames_Throws_WhenPropertiesDontMatchQueryResult()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var errMessage = string.Format(Resources.InvalidQueryOrientedProperties, "undefined", typeof(TestModelWithCustomNames).FullName);

        mockReader.Setup(r => r.FieldCount).Returns(2);
        mockReader.Setup(r => r.GetName(0)).Returns("name_column");
        mockReader.Setup(r => r.GetName(1)).Returns("undefined");

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            DataSerializer.GetQueryOrientedNames<TestModelWithCustomNames>(mockReader.Object);
        });

        Assert.Equal(errMessage, exception.Message);
    }

    [Fact]
    public void Serialize_ReturnsCorrectSerializationField_ForJsonAttribute()
    {
        // Arrange
        var property = typeof(TestModelWithAttributes).GetProperty(nameof(TestModelWithAttributes.JsonProperty));

        // Act
        var result = DataSerializer.Serialize(property!);

        // Assert
        Assert.IsType<JsonField>(result);
        Assert.Equal(nameof(TestModelWithAttributes.JsonProperty), result.Name);
    }

    [Fact]
    public void Serialize_ReturnsCorrectSerializationField_ForEnumAttribute()
    {
        // Arrange
        var property = typeof(TestModelWithAttributes).GetProperty(nameof(TestModelWithAttributes.EnumProperty));

        // Act
        var result = DataSerializer.Serialize(property!);

        // Assert
        Assert.IsType<EnumField>(result);
        Assert.Equal(nameof(TestModelWithAttributes.EnumProperty), result.Name);
    }

    [Fact]
    public void Serialize_ReturnsCorrectSerializationField_ForDefault()
    {
        // Arrange
        var property = typeof(TestModelWithAttributes).GetProperty(nameof(TestModelWithAttributes.DefaultProperty));

        // Act
        var result = DataSerializer.Serialize(property!);

        // Assert
        Assert.IsType<SerializationField>(result);
        Assert.Equal(nameof(TestModelWithAttributes.DefaultProperty), result.Name);
    }

    [Fact]
    public async Task Object_DeserializesObjectsWithAttributes()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        var data = new Dictionary<string, object>
        {
            { "name_column", "John" },
            { "age_column", 30 },
            { "LastName", null! }
        };
        var keys = data.Keys.ToList();

        mockReader.Setup(r => r.FieldCount).Returns(data.Count);
        mockReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns<string>(keys.IndexOf);
        mockReader.Setup(r => r.GetName(It.IsAny<int>())).Returns<int>(i => data.ElementAt(i).Key);
        mockReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns<int>(i => data.ElementAt(i).Value);
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);

        // Act
        var result = await DataSerializer.Object<TestModelWithCustomNames>(lazyReader, MappingOrientation.ObjectOriented, null);

        // Assert
        var model = result.First();

        Assert.Single(result);
        Assert.Equal("John", model.Name);
        Assert.Equal(30, model.Age);
    }

    [Fact]
    public async Task Json_HandlesNullValues()
    {
        // Arrange
        var jsonData = """{"Name":null}""";

        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.Setup(r => r.GetString(0)).Returns(jsonData);

        // Act
        var result = await DataSerializer.Json<TestModel>(lazyReader);

        // Assert
        Assert.Single(result);
        Assert.Null(result.First().Name);
    }

    [Theory]
    [InlineData("invalid", typeof(FormatException))]
    [InlineData(null, typeof(InvalidCastException))]
    public async Task Primitive_ThrowsExceptionForInvalidData(object data, Type exceptionType)
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.Setup(r => r.GetValue(0)).Returns(data);

        // Act & Assert
        await Assert.ThrowsAsync(exceptionType, async () => 
        {
            await DataSerializer.Primitive<int>(lazyReader);
        });
    }

    [Fact]
    public async Task Primitive_HandlesLargeDataset()
    {
        // Arrange
        const int dataSize = 10000;
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        var sequence = mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()));
        for (int i = 0; i < dataSize; i++)
        {
            sequence = sequence.ReturnsAsync(true);
        }
        sequence.ReturnsAsync(false);

        var valueSequence = mockReader.SetupSequence(r => r.GetValue(0));
        for (int i = 0; i < dataSize; i++)
        {
            valueSequence = valueSequence.Returns(i);
        }

        // Act
        var result = await DataSerializer.Primitive<int>(lazyReader);

        // Assert
        Assert.Equal(dataSize, result.Count());
        for (int i = 0; i < dataSize; i++)
        {
            Assert.Equal(i, result.ElementAt(i));
        }
    }

    [Fact]
    public async Task Object_HandlesObjectOrientedMapping()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.Setup(r => r.FieldCount).Returns(1);
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(0);
        mockReader.Setup(r => r.GetValue(0)).Returns("John");

        // Act
        var result = await DataSerializer.Object<TestModel>(lazyReader, MappingOrientation.ObjectOriented, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result.First().Name);
    }

    [Fact]
    public async Task Object_HandlesQueryOrientedMapping()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.Setup(r => r.FieldCount).Returns(1);
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.Setup(r => r.GetName(It.IsAny<int>())).Returns("Name");
        mockReader.Setup(r => r.GetValue(0)).Returns("John");

        // Act
        var result = await DataSerializer.Object<TestModel>(lazyReader, MappingOrientation.QueryOriented, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("John", result.First().Name);
    }

    [Fact]
    public async Task Object_HandlesCustomAttributes()
    {
        // Arrange
        var jsonData = """{"InnerName":"InnerValue"}""";

        var mockReader = new Mock<DbDataReader>();
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        mockReader.Setup(r => r.FieldCount).Returns(1);
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);
        mockReader.Setup(r => r.GetOrdinal(It.IsAny<string>())).Returns(0);
        mockReader.Setup(r => r.GetValue(0)).Returns(jsonData);

        // Act
        var result = await DataSerializer.Object<TestModelWithInnerObject>(lazyReader, MappingOrientation.ObjectOriented, null);

        // Assert
        Assert.Single(result);
        Assert.Equal("InnerValue", result.First().InnerObject?.InnerName);
    }

    private class TestModel
    {
        public string? Name { get; set; }
    }

    private class TestModelWithCustomNames
    {
        [Column("name_column")]
        public string? Name { get; set; }

        public string? LastName { get; set; }

        [Column("age_column")]
        public int Age { get; set; }
    }

    private class TestModelWithInnerObject
    {
        [Json]
        public InnerModel? InnerObject { get; set; }
    }

    private class TestModelWithAttributes
    {
        [Json]
        public string? JsonProperty { get; set; }

        [Enumeration]
        public string? EnumProperty { get; set; }

        public string? DefaultProperty { get; set; }
    }

    private class InnerModel
    {
        public string? InnerName { get; set; }
    }
}
