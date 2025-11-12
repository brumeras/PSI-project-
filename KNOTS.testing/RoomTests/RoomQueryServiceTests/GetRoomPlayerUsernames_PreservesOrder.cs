using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomPlayerUsernamesTests;

/// <summary>
/// Tests that GetRoomPlayerUsernames preserves the order of players in the room.
/// Verifies the returned list maintains the same order as the Players collection.
/// </summary>
public class GetRoomPlayerUsernames_PreservesOrder : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "ORDER01";
        var room = CreateTestRoom(roomCode, "Alpha", "Alpha", "Beta", "Gamma");
        RoomRepository.AddRoom(room);

        // Act
        var result = RoomQueryService.GetRoomPlayerUsernames(roomCode);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Alpha", result[0]);
        Assert.Equal("Beta", result[1]);
        Assert.Equal("Gamma", result[2]);
    }
}