using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomInfoTests;

/// <summary>
/// Tests that GetRoomInfo returns the correct room when it exists.
/// Verifies the room is retrieved from the repository using the provided room code.
/// </summary>
public class GetRoomInfo_ReturnsRoom : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "TEST01";
        var room = CreateTestRoom(roomCode, "Host", "Host", "Player1");
        RoomRepository.AddRoom(room);

        // Act
        var result = RoomQueryService.GetRoomInfo(roomCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roomCode, result.RoomCode);
        Assert.Equal("Host", result.Host);
        Assert.Equal(2, result.Players.Count);
    }
}