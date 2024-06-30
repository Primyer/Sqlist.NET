using Moq;

using Sqlist.NET.Utilities;
using System.Data.Common;

namespace Sqlist.NET.Tests;
public class LazyDbDataReaderTests
{
    [Fact]
    public void Constructor_InitializesReader_Property()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();

        // Act
        var lazyReader = new LazyDbDataReader(mockReader.Object);

        // Assert
        Assert.NotNull(lazyReader.Reader);
    }

    [Fact]
    public async Task IterateAsync_ReadsData_AndInvokesAction()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);

        var lazyReader = new LazyDbDataReader(mockReader.Object);

        var actionInvokedCount = 0;

        // Act
        await lazyReader.IterateAsync(_ => actionInvokedCount++, CancellationToken.None);

        // Assert
        Assert.Equal(2, actionInvokedCount);
        mockReader.Verify(r => r.ReadAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task IterateAsync_TriggersFetchedEvent()
    {
        // Arrange
        var mockReader = new Mock<DbDataReader>();
        mockReader.SetupSequence(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true)
                  .ReturnsAsync(false);

        var lazyReader = new LazyDbDataReader(mockReader.Object);

        var fetchedInvoked = false;
        lazyReader.Fetched += () => fetchedInvoked = true;

        // Act
        await lazyReader.IterateAsync(_ => { }, CancellationToken.None);

        // Assert
        Assert.True(fetchedInvoked);
    }
}
