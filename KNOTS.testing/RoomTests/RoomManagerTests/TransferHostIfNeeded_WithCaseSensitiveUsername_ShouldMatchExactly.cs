using Xunit;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests.TransferHostIfNeededTests;

/// <summary>
/// Tests that TransferHostIfNeeded performs case-sensitive username comparison.
/// Verifies that usernames differing only by case are treated as different users.
/// </summary>
public class TransferHostIfNeeded_WithCaseSensitiveUsername_ShouldMatchExactly : RoomManagerTestBase
{
    [Fact]
    public void Test()
    {
        // Arrange
        var hostUsername = "Host";
        var disconnectedUsername = "host"; // Different case
        var room = CreateTestRoom("ROOM06", hostUsername, hostUsername, "Player2");
        var originalHost = room.Host;

        // Act
        RoomManager.TransferHostIfNeeded(room, disconnectedUsername);

        // Assert
        Assert.Equal(originalHost, room.Host);
    }
}