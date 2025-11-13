using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomPlayerUsernamesTests;

/// <summary>
/// Tests that GetRoomPlayerUsernames returns an empty list when the room has no players.
/// Verifies the method handles empty player lists correctly.
/// </summary>
public class GetRoomPlayerUsernames_EmptyRoom : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "EMPTY01";
        var room = CreateTestRoom(roomCode, "Host");
        room.Players.Clear(); 
        RoomRepository.AddRoom(room);

        // Act
        var result = RoomQueryService.GetRoomPlayerUsernames(roomCode);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}