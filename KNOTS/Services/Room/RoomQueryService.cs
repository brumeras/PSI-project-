using System.Collections.Generic;
using System.Linq;

namespace KNOTS.Services;

/// <summary>
/// Provides read-only access to room and player information.
/// </summary>
/// <remarks>
/// This service centralizes all query-related operations for retrieving game room data,
/// such as player lists, room details, and host information, without modifying state.
/// </remarks>
public class RoomQueryService {
    private readonly RoomRepository _roomRepository;
    private readonly PlayerMappingRepository _playerMappingRepository;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RoomQueryService"/> class.
    /// </summary>
    /// <param name="roomRepository">The repository used to access stored game rooms.</param>
    /// <param name="playerMappingRepository">The repository used to track player-to-room mappings.</param>
    public RoomQueryService(RoomRepository roomRepository, PlayerMappingRepository playerMappingRepository) {
        _roomRepository = roomRepository;
        _playerMappingRepository = playerMappingRepository;
    }
    
    /// <summary>
    /// Retrieves full information about a specific room.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <returns>The <see cref="GameRoom"/> instance if found; otherwise, <c>null</c>.</returns>
    public GameRoom? GetRoomInfo(string roomCode) { return _roomRepository.GetRoom(roomCode); }
    
    /// <summary>
    /// Retrieves a list of usernames of all players currently in a given room.
    /// </summary>
    /// <param name="roomCode">The unique code identifying the room.</param>
    /// <returns>A list of player usernames. Returns an empty list if the room is not found.</returns>
    public List<string> GetRoomPlayerUsernames(string roomCode) {
        var room = _roomRepository.GetRoom(roomCode);
        return room?.Players.Select(p => p.Username).ToList() ?? new List<string>();
    }
    
    /// <summary>
    /// Gets the username associated with a given connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player.</param>
    /// <returns>The player's username, or an empty string if not found.</returns>
    public string GetPlayerUsername(string connectionId) { return _playerMappingRepository.GetPlayerUsername(connectionId) ?? ""; }
    
    /// <summary>
    /// Retrieves the room code associated with a player's connection ID.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player.</param>
    /// <returns>The room code, or <c>null</c> if the player is not assigned to any room.</returns>
    public string? GetPlayerRoomCode(string connectionId) { return _playerMappingRepository.GetPlayerRoomCode(connectionId); }
    
    /// <summary>
    /// Determines whether a given player is the host of their current room.
    /// </summary>
    /// <param name="connectionId">The connection ID of the player.</param>
    /// <returns><c>true</c> if the player is the host; otherwise, <c>false</c>.</returns>
    public bool IsPlayerHost(string connectionId) {
        var roomCode = _playerMappingRepository.GetPlayerRoomCode(connectionId);
        if (roomCode == null) return false;

        var room = _roomRepository.GetRoom(roomCode);
        if (room == null) return false;

        var username = _playerMappingRepository.GetPlayerUsername(connectionId);
        return room.Host == username;
    }
    
    /// <summary>
    /// Retrieves a list of all currently active rooms.
    /// </summary>
    /// <returns>A list of <see cref="GameRoom"/> instances.</returns>
    public List<GameRoom> GetAllRooms() { return _roomRepository.GetAllRooms(); }
}