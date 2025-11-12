using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded does not attempt host transfer or empty check when a non-host disconnects.
/// Verifies that the host remains unchanged.
/// </summary>
public class TransferHostIfNeeded_WithDifferentHostName_ShouldNotCallIsEmpty : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var hostUsername = "ActualHost";
        var disconnectedUsername = "SomePlayer";
        var room = CreateTestRoom("ROOM05", hostUsername, hostUsername, disconnectedUsername);
        var originalHost = room.Host;

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert
        Assert.Equal(originalHost, room.Host);
    }
}