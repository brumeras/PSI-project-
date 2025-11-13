using KNOTS.Services;
using System.Collections.Generic;

namespace KNOTS.Tests.Services.RoomQueryServiceTests;

/// <summary>
/// Base class for RoomQueryService unit tests.
/// Provides common setup and helper methods for test classes using real instances.
/// </summary>
public abstract class RoomQueryServiceTestBase
{
    /// <summary>
    /// Real repository instance for managing game rooms in tests.
    /// </summary>
    protected RoomRepository RoomRepository { get; }
    
    /// <summary>
    /// Real repository instance for managing player connection mappings in tests.
    /// </summary>
    protected PlayerMappingRepository PlayerMappingRepository { get; }
    
    /// <summary>
    /// Instance of RoomQueryService under test with real dependencies.
    /// </summary>
    protected RoomQueryService RoomQueryService { get; }

    /// <summary>
    /// Initializes real repository instances and creates RoomQueryService for testing.
    /// </summary>
    protected RoomQueryServiceTestBase()
    {
        RoomRepository = new RoomRepository();
        PlayerMappingRepository = new PlayerMappingRepository();
        RoomQueryService = new RoomQueryService(RoomRepository, PlayerMappingRepository);
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