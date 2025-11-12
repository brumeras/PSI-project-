using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomPlayerUsernamesTests;

/// <summary>
/// Tests that GetRoomPlayerUsernames returns a list of player usernames for an existing room.
/// Verifies all player usernames are extracted correctly from the room.
/// </summary>
public class GetRoomPlayerUsernames_ReturnsList : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "GAME01";
        var room = CreateTestRoom(roomCode, "Host", "Host", "Player1", "Player2");
        RoomRepository.AddRoom(room);

        // Act
        var result = RoomQueryService.GetRoomPlayerUsernames(roomCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("Host", result);
        Assert.Contains("Player1", result);
        Assert.Contains("Player2", result);
    }
}