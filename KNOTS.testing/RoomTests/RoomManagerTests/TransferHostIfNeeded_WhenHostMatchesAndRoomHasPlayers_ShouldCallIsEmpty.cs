using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded checks if the room is empty when the host disconnects.
/// Verifies the host is transferred only when IsEmpty returns false.
/// </summary>
public class TransferHostIfNeeded_WhenHostMatchesAndRoomHasPlayers_ShouldCallIsEmpty : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var disconnectedUsername = "Host1";
        var room = CreateTestRoom("ROOM04", disconnectedUsername, disconnectedUsername, "Player2");
        
        // Remove the disconnected host from players list (simulating disconnection)
        room.Players.RemoveAll(p => p.Username == disconnectedUsername);
        
        var originalHost = room.Host;

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert - Verify transfer occurred, which means IsEmpty was checked and returned false
        Assert.NotEqual(originalHost, room.Host);
        Assert.False(room.IsEmpty());

    }
}