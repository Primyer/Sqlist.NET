using System.Data;
using System.Data.Common;

using Microsoft.Extensions.Logging;

using Moq;

using Sqlist.NET.Infrastructure;

namespace Sqlist.NET.Migration.Tests;

public class MigrationTransactionManagerTests
{
    private const string DbName = "testDb";
    private const string OldName = "testDb_old";

    private readonly Mock<DbConnection> _mockConnection = new Mock<DbConnection>();
    private readonly Mock<IMigrationService> _mockMigrationService = new Mock<IMigrationService>();
    private readonly Mock<ILogger<MigrationTransactionManager>> _mockLogger =
        new Mock<ILogger<MigrationTransactionManager>>();
    
    private readonly MigrationTransactionManager _migrationTransaction;

    public MigrationTransactionManagerTests()
    {
        var mockDbContext = new Mock<IDbContext>();
        
        _mockConnection.Setup(x => x.State).Returns(ConnectionState.Open);
        mockDbContext.Setup(db => db.Connection).Returns(_mockConnection.Object);
        
        _migrationTransaction = new MigrationTransactionManager(
            mockDbContext.Object, _mockMigrationService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task PrepareDatabaseForMigrationAsync_ShouldHandleClosedConnectionState()
    {
        // Arrange - Connection initially closed
        _mockConnection.Setup(x => x.State).Returns(ConnectionState.Closed);

        // Act
        await _migrationTransaction.PrepareDatabaseForMigrationAsync(DbName, OldName, CancellationToken.None);

        // Assert
        _mockMigrationService.Verify(x => x.RenameDatabaseAsync(DbName, OldName, CancellationToken.None), Times.Once);
        _mockMigrationService.Verify(x => x.CreateDatabaseAsync(DbName, CancellationToken.None), Times.Once);
        _mockConnection.Verify(x => x.ChangeDatabaseAsync(DbName, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task PrepareDatabaseForMigrationAsync_ShouldRenameAndCreateDatabase()
    {
        // Act
        await _migrationTransaction.PrepareDatabaseForMigrationAsync(DbName, OldName, CancellationToken.None);

        // Assert
        _mockMigrationService.Verify(x => x.RenameDatabaseAsync(DbName, OldName, CancellationToken.None), Times.Once);
        _mockMigrationService.Verify(x => x.CreateDatabaseAsync(DbName, CancellationToken.None), Times.Once);
        _mockConnection.Verify(x => x.ChangeDatabaseAsync(DbName, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RollbackMigrationAsync_ShouldRenameDatabase_WhenCreateDatabaseFails()
    {
        // Arrange
        _mockMigrationService.Setup(x => x.CreateDatabaseAsync(DbName, CancellationToken.None))
            .ThrowsAsync(new Exception("Database creation failed.")); // Simulate failure

        // Act
        try
        {
            await _migrationTransaction.PrepareDatabaseForMigrationAsync(DbName, OldName, CancellationToken.None);
        }
        catch
        {
            await _migrationTransaction.RollbackMigrationAsync(DbName, OldName, CancellationToken.None);
        }

        // Assert
        _mockMigrationService.Verify(x => x.RenameDatabaseAsync(OldName, DbName, CancellationToken.None), Times.Once);
        _mockMigrationService.Verify(x => x.DeleteDatabaseAsync(DbName, CancellationToken.None), Times.Never);
    }

    [Fact]
    public async Task RollbackMigrationAsync_ShouldDeleteDatabaseAndRestoreOldOne()
    {
        // Act
        await _migrationTransaction.PrepareDatabaseForMigrationAsync(DbName, OldName, CancellationToken.None);
        await _migrationTransaction.RollbackMigrationAsync(DbName, OldName, CancellationToken.None);

        // Assert
        _mockMigrationService.Verify(x => x.DeleteDatabaseAsync(DbName, CancellationToken.None), Times.Once);
        _mockMigrationService.Verify(x => x.RenameDatabaseAsync(OldName, DbName, CancellationToken.None), Times.Once);
    }
}