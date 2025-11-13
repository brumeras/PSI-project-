using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomQueryServiceTests.GetPlayerUsernameTests;

/// <summary>
/// Tests that GetPlayerUsername returns an empty string when the connection ID is not found.
/// Verifies the method handles non-existent mappings gracefully.
/// </summary>
public class GetPlayerUsername_ReturnsEmpty : RoomQueryServiceTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var connectionId = "unknown_connection";
        // Don't add any mapping to the repository

        // Act
        var result = RoomQueryService.GetPlayerUsername(connectionId);

        // Assert
        Assert.Equal("", result);
    }
}