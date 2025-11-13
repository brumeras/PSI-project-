using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded transfers host when the current host disconnects from a non-empty room.
/// Verifies the room's host is reassigned when host leaves but other players remain.
/// </summary>
public class TransferHostIfNeeded_WhenHostDisconnectsAndRoomNotEmpty_ShouldTransferHost : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var disconnectedUsername = "Host1";
        var room = CreateTestRoom("ROOM04", disconnectedUsername, disconnectedUsername, "Player2");
        var originalHost = room.Host;
        
        // Remove the disconnected host from players list
        room.Players.RemoveAll(p => p.Username == disconnectedUsername);

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert
        Assert.NotEqual(originalHost, room.Host);
        Assert.False(room.IsEmpty());
    }
}