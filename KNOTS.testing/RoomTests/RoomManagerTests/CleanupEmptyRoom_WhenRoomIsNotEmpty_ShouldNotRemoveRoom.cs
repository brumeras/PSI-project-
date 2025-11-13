using Xunit;
using KNOTS.Services;
using KNOTS.Models;

namespace KNOTS.Tests.Services.RoomManagerTests.CleanupEmptyRoomTests;

/// <summary>
/// Tests that CleanupEmptyRoom does not remove a room when it still has players.
/// </summary>
public class CleanupEmptyRoom_WhenRoomIsNotEmpty_ShouldNotRemoveRoom
{
    [Fact]
    public void Test()
    {
        // Arrange
        var repo = new RoomRepository();
        var codeGen = new RoomCodeGenerator();
        var manager = new RoomManager(repo, codeGen);

        var room = new GameRoom
        {
            RoomCode = "ROOM2",
            Host = "HostUser",
            Players = new List<GamePlayer>
            {
                new GamePlayer("conn1", "HostUser")
            }
        };
        repo.AddRoom(room);

        // Act
        manager.CleanupEmptyRoom("ROOM2");

        // Assert
        var existingRoom = repo.GetRoom("ROOM2");
        Assert.NotNull(existingRoom);
        Assert.Equal("ROOM2", existingRoom.RoomCode);
    }
}