using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded does not transfer host when the room becomes empty.
/// Verifies no host reassignment occurs when the host is the last player to leave.
/// </summary>
public class TransferHostIfNeeded_WhenHostDisconnectsAndRoomIsEmpty_ShouldNotTransferHost : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var disconnectedUsername = "LastPlayer";
        var room = CreateTestRoom("ROOM03", disconnectedUsername, disconnectedUsername);
        room.Players.Clear(); // Make room empty
        var originalHost = room.Host;

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert
        Assert.Equal(originalHost, room.Host);
    }
}