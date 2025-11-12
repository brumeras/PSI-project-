using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded does not transfer host when a non-host player disconnects.
/// Verifies the room's host remains unchanged when a regular player leaves.
/// </summary>
public class TransferHostIfNeeded_WhenNonHostDisconnects_ShouldNotTransferHost : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var hostUsername = "CurrentHost";
        var disconnectedUsername = "RegularPlayer";
        var room = CreateTestRoom("ROOM02", hostUsername, hostUsername, disconnectedUsername);
        var originalHost = room.Host;

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert
        Assert.Equal(originalHost, room.Host);
    }
}