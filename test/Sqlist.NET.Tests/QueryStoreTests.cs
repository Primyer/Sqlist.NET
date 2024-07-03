using Moq;

using Sqlist.NET.Infrastructure;

using System.Data;
using System.Data.Common;

namespace Sqlist.NET.Tests;
public class QueryStoreTests
{
    private readonly Mock<QueryStore> _mockQueryStore;
    private readonly Mock<ICommand> _mockCommand;
    private readonly DbOptions _options;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QueryStoreTests"/> class.
    /// </summary>
    public QueryStoreTests()
    {
        _options = new DbOptions();
        _mockQueryStore = new Mock<QueryStore>(_options) { CallBase = true };
        _mockCommand = new Mock<ICommand>();

        _mockQueryStore.Setup(x => x.CreateCommand(It.IsAny<DbConnection>()))
                       .Returns(_mockCommand.Object);
    }

    [Fact]
    public void Execute_CallsExecuteAsync()
    {
        // Arrange
        _mockQueryStore.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        // Act
        var result = _mockQueryStore.Object.Execute("SELECT 1");

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task ExecuteAsync_CallsExecuteNonQueryAsync()
    {
        // Arrange
        _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1);

        // Act
        var result = await _mockQueryStore.Object.ExecuteAsync("SELECT 1");

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Retrieve_CallsRetrieveAsync()
    {
        // Arrange
        var expectedData = new List<int> { 1, 2, 3 };

        _mockQueryStore.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Action<int>>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedData);

        // Act
        var result = _mockQueryStore.Object.Retrieve<int>("SELECT 1");

        // Assert
        Assert.Equal(expectedData, result);
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsData()
    {
        // Arrange
        var expectedData = new List<int> { 1, 2, 3 };

        _mockQueryStore.Setup(x => x.RetrieveAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<Action<int>>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedData);

        // Act
        var result = await _mockQueryStore.Object.RetrieveAsync<int>("SELECT 1");

        // Assert
        Assert.Equal(expectedData, result);
    }

    [Fact]
    public void ExecuteScalar_CallsExecuteScalarAsync()
    {
        // Arrange
        var expectedValue = 1;

        _mockQueryStore.Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedValue);

        // Act
        var result = _mockQueryStore.Object.ExecuteScalar("SELECT 1");

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task ExecuteScalarAsync_ReturnsScalarValue()
    {
        // Arrange
        var expectedValue = 1;

        _mockQueryStore.Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(expectedValue);

        // Act
        var result = await _mockQueryStore.Object.ExecuteScalarAsync("SELECT 1");

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void ExecuteReader_CallsExecuteReaderAsync()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();

        _mockCommand.Setup(x => x.ExecuteReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockReader.Object);

        // Act
        var result = _mockQueryStore.Object.ExecuteReader("SELECT 1");

        // Assert
        Assert.NotNull(result);
        Assert.True(mockReader.Object == result, "The tested method didn't return the expected object.");

        _mockQueryStore.Verify(x => x.ExecuteReaderAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<int?>(), It.IsAny<CommandType?>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task ExecuteReaderAsync_ReturnsDbDataReader()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();

        _mockCommand.Setup(x => x.ExecuteReaderAsync(It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockReader.Object);

        // Act
        var result = await _mockQueryStore.Object.ExecuteReaderAsync("SELECT 1");

        // Assert
        Assert.NotNull(result);
        Assert.True(mockReader.Object == result, "The tested method didn't return the expected object.");
    }

    [Fact]
    public async Task ExecuteAsync_CallsOnCommandCompleted()
    {
        // Arrange
        var wasCalled = false;

        _mockCommand.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(1);

        _mockQueryStore.Object.OnCompleted += () => wasCalled = true;

        // Act
        await _mockQueryStore.Object.ExecuteAsync("SELECT 1");

        // Assert
        Assert.True(wasCalled);
    }
}
