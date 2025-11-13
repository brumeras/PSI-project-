using Xunit;
using KNOTS.Services;
using KNOTS.Models;

namespace KNOTS.Tests.Services.RoomManagerTests.CleanupEmptyRoomTests;

/// <summary>
/// Tests that CleanupEmptyRoom removes a room from the repository
/// when the room exists and is empty.
/// </summary>
public class CleanupEmptyRoom_WhenRoomIsEmpty_ShouldRemoveRoom
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
            RoomCode = "ROOM1",
            Host = "HostUser",
            Players = new List<GamePlayer>()
        };
        repo.AddRoom(room);

        // Act
        manager.CleanupEmptyRoom("ROOM1");

        // Assert
        Assert.Null(repo.GetRoom("ROOM1"));
    }
}