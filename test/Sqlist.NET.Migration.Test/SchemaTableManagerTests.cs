using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Sqlist.NET.Infrastructure;
using Sqlist.NET.Migration.Infrastructure;

namespace Sqlist.NET.Migration.Tests;

public class SchemaTableManagerTests
{
    private readonly Mock<IDbContext> _mockDbContext = new Mock<IDbContext>();
    private readonly Mock<IMigrationService> _mockMigrationService = new Mock<IMigrationService>();
    private readonly Mock<IOptions<MigrationOptions>> _mockOptions = new Mock<IOptions<MigrationOptions>>();
    private readonly Mock<ILogger<SchemaTableManager>> _mockLogger = new Mock<ILogger<SchemaTableManager>>();
    private readonly SchemaTableManager _schemaTableManager;

    public SchemaTableManagerTests()
    {
        var mockTypeMapper = new Mock<ITypeMapper>();
        
        mockTypeMapper.Setup(x => x.TypeName<It.IsAnyType>())
            .Returns(new InvocationFunc(call => call.Method.GetGenericArguments()[0].Name));
        
        _mockDbContext.Setup(x => x.TypeMapper).Returns(mockTypeMapper.Object);
        _mockOptions.Setup(x => x.Value).Returns(new MigrationOptions());
        
        _schemaTableManager = new SchemaTableManager(
            _mockDbContext.Object, _mockMigrationService.Object, _mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task RetrieveSchemaDetailsAsync_ShouldReturnMigrationOperationInfo_WithCurrentVersion()
    {
        // Arrange
        var expectedVersion = new Version("1.0.0");
        _mockMigrationService.Setup(x => x.DoesSchemaTableExistAsync(CancellationToken.None)).ReturnsAsync(true);
        _mockMigrationService.Setup(x => x.GetLastSchemaPhaseAsync(CancellationToken.None))
            .ReturnsAsync(new SchemaPhase { Version = expectedVersion.ToString() });

        // Act
        var result = await _schemaTableManager.RetrieveSchemaDetailsAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedVersion, result.CurrentVersion);
    }

    [Fact]
    public async Task RetrieveSchemaDetailsAsync_ShouldReturnMigrationOperationInfo_WithNullCurrentVersion_WhenSchemaTableDoesNotExist()
    {
        // Arrange
        _mockMigrationService.Setup(x => x.DoesSchemaTableExistAsync(CancellationToken.None)).ReturnsAsync(false);

        // Act
        var result = await _schemaTableManager.RetrieveSchemaDetailsAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CurrentVersion);
    }

    [Fact]
    public void GetSchemaTableDefinition_ShouldReturnMigrationPhase_WithCreateTableDefinition()
    {
        // Arrange
        const string tableName = "schema_table";
        _mockOptions.Object.Value.SchemaTable = tableName;
        
        // Act
        var phase = _schemaTableManager.GetSchemaTableDefinition();

        // Assert
        Assert.NotNull(phase);
        Assert.NotEmpty(phase.Guidelines.Create);
        Assert.True(phase.Guidelines.Create.ContainsKey(tableName));
        Assert.NotEmpty(phase.Guidelines.Create[tableName].Columns);
    }
}