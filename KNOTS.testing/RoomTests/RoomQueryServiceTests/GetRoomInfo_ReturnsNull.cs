using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomInfoTests;

/// <summary>
/// Tests that GetRoomInfo returns null when the room does not exist.
/// Verifies the method handles non-existent rooms gracefully.
/// </summary>
public class GetRoomInfo_ReturnsNull : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "NOTFOUND";

        // Act
        var result = RoomQueryService.GetRoomInfo(roomCode);

        // Assert
        Assert.Null(result);
    }
}