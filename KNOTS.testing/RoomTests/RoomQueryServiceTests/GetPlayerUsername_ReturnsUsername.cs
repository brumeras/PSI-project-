using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetPlayerUsernameTests;

/// <summary>
/// Tests that GetPlayerUsername returns the correct username for a valid connection ID.
/// Verifies the username is retrieved from the player mapping repository.
/// </summary>
public class GetPlayerUsername_ReturnsUsername : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var connectionId = "conn_123";
        var username = "TestPlayer";
        var roomCode = "ROOM01";
        PlayerMappingRepository.AddPlayer(connectionId, username, roomCode);

        // Act
        var result = RoomQueryService.GetPlayerUsername(connectionId);

        // Assert
        Assert.Equal(username, result);
    }
}