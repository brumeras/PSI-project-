using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetRoomPlayerUsernamesTests;

/// <summary>
/// Tests that GetRoomPlayerUsernames returns an empty list when the room does not exist.
/// Verifies the method handles null rooms gracefully without throwing exceptions.
/// </summary>
public class GetRoomPlayerUsernames_NullRoom : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var roomCode = "NOTFOUND";
        

        // Act
        var result = RoomQueryService.GetRoomPlayerUsernames(roomCode);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}