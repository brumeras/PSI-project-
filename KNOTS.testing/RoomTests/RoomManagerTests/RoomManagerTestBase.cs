using Moq;
using KNOTS.Services;

namespace KNOTS.Tests.Services.RoomManagerTests;

/// <summary>
/// Base class for RoomManager unit tests.
/// Provides common mock setup and helper methods for test classes.
/// </summary>
public abstract class RoomManagerTestBase
{
    /// <summary>
    /// Mock repository for managing game rooms.
    /// </summary>
    protected Mock<RoomRepository> MockRoomRepository { get; }
    
    /// <summary>
    /// Mock generator for creating unique room codes.
    /// </summary>
    protected Mock<RoomCodeGenerator> MockCodeGenerator { get; }
    
    /// <summary>
    /// Instance of RoomManager under test with mocked dependencies.
    /// </summary>
    protected RoomManager RoomManager { get; }

    /// <summary>
    /// Initializes mock objects and creates RoomManager instance for testing.
    /// </summary>
    protected RoomManagerTestBase()
    {
        MockRoomRepository = new Mock<RoomRepository>();
        MockCodeGenerator = new Mock<RoomCodeGenerator>();
        RoomManager = new RoomManager(MockRoomRepository.Object, MockCodeGenerator.Object);
    }

    /// <summary>
    /// Creates a test GameRoom with specified parameters.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <param name="host">The username of the room host.</param>
    /// <param name="playerUsernames">Usernames of players to add to the room.</param>
    /// <returns>A configured GameRoom instance for testing.</returns>
    protected GameRoom CreateTestRoom(string roomCode, string host, params string[] playerUsernames)
    {
        var players = new List<GamePlayer>();
        for (int i = 0; i < playerUsernames.Length; i++)
        {
            players.Add(new GamePlayer($"conn_{i}", playerUsernames[i]));
        }

        return new GameRoom
        {
            RoomCode = roomCode,
            Host = host,
            Players = players
        };
    }
}