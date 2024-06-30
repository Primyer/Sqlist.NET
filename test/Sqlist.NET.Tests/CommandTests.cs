using Moq;
using Moq.Protected;

using Sqlist.NET.Infrastructure;

using System.Data;
using System.Data.Common;

namespace Sqlist.NET.Tests;

public class CommandTests
{
    private readonly Mock<DbContextBase> _mockDbContext;
    private readonly Mock<DbConnection> _mockConnection;
    private readonly Mock<DbCommand> _mockDbCommand;
    private readonly Command _command;

    public CommandTests()
    {
        var options = new DbOptions();

        _mockDbCommand = new Mock<DbCommand>() { CallBase = true };
        _mockDbCommand.Protected()
                      .Setup<DbParameter>("CreateDbParameter")
                      .Returns(new Mock<DbParameter>().Object);

        _mockConnection = new Mock<DbConnection>();
        _mockConnection.Protected()
                       .Setup<DbCommand>("CreateDbCommand")
                       .Returns(_mockDbCommand.Object);

        _mockDbContext = new Mock<DbContextBase>(options);
        _mockDbContext.Setup(db => db.Connection).Returns(_mockConnection.Object);

        _command = new Command(_mockDbContext.Object);
    }

    [Fact]
    public void Statement_ShouldSetCommandText()
    {
        // Arrange
        var sql = "SELECT * FROM Table";

        // Act
        _command.Statement = sql;

        // Assert
        _mockDbCommand.VerifySet(cmd => cmd.CommandText = sql, Times.Once);
    }

    [Fact]
    public void Parameters_ShouldConfigureParameters()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "param1", 1 },
            { "param2", "value" }
        };

        var mockCollection = new Mock<DbParameterCollection>();

        _mockDbCommand.Protected()
                      .Setup<DbParameterCollection>("DbParameterCollection")
                      .Returns(mockCollection.Object);

        // Act
        _command.Parameters = parameters;

        // Assert
        mockCollection.Verify(x => x.Add(It.IsAny<object>()), Times.Exactly(parameters.Count));
    }

    [Fact]
    public void Timeout_ShouldSetCommandTimeout()
    {
        // Arrange
        var timeout = 30;

        // Act
        _command.Timeout = timeout;

        // Assert
        _mockDbCommand.VerifySet(cmd => cmd.CommandTimeout = timeout, Times.Once);
    }

    [Fact]
    public void Type_ShouldSetCommandType()
    {
        // Arrange
        var type = CommandType.StoredProcedure;

        // Act
        _command.Type = type;

        // Assert
        _mockDbCommand.VerifySet(cmd => cmd.CommandType = type, Times.Once);
    }

    [Fact]
    public async Task ExecuteNonQueryAsync_ShouldOpenConnectionIfClosed_AndExecuteCommand()
    {
        // Arrange
        _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Closed);
        _mockDbCommand.Setup(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _command.ExecuteNonQueryAsync();

        // Assert
        _mockConnection.Verify(conn => conn.Open(), Times.Once);
        _mockDbCommand.Verify(cmd => cmd.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteScalarAsync_ShouldOpenConnectionIfClosed_AndExecuteCommand()
    {
        // Arrange
        _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Closed);
        _mockDbCommand.Setup(cmd => cmd.ExecuteScalarAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _command.ExecuteScalarAsync();

        // Assert
        _mockConnection.Verify(conn => conn.Open(), Times.Once);
        _mockDbCommand.Verify(cmd => cmd.ExecuteScalarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteReaderAsync_ShouldOpenConnectionIfClosed_AndExecuteCommand()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        var methodName = "ExecuteDbDataReaderAsync";

        _mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Closed);

        _mockDbCommand.Protected()
                      .Setup<Task<DbDataReader>>(methodName, It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>())
                      .ReturnsAsync(mockReader.Object);

        // Act
        var result = await _command.ExecuteReaderAsync();

        // Assert
        _mockConnection.Verify(conn => conn.Open(), Times.Once);
        _mockDbCommand.Protected()
                      .Verify<Task<DbDataReader>>(methodName, Times.Once(), It.IsAny<CommandBehavior>(), It.IsAny<CancellationToken>());
    }
}