using Xunit;
using KNOTS.Services;
using KNOTS.Models;
using System.Linq;

namespace KNOTS.Tests.Services.RoomManagerTests.CleanupEmptyRoomTests;

/// <summary>
/// Tests that CleanupEmptyRoom can be called repeatedly on the same room
/// without throwing exceptions or causing state corruption.
/// </summary>
public class CleanupEmptyRoom_WhenCalledMultipleTimesOnSameRoom_ShouldNotThrow
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
            RoomCode = "RETRY1",
            Host = "Host",
            Players = new List<GamePlayer>()
        };
        repo.AddRoom(room);

        // Act
        manager.CleanupEmptyRoom("RETRY1");
        var exception = Record.Exception(() => manager.CleanupEmptyRoom("RETRY1"));

        // Assert
        Assert.Null(exception);
        
        Assert.Empty(repo.GetAllRoomCodes());
    }
}